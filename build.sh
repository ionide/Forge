#!/usr/bin/env bash

set -eu
set -o pipefail

cd `dirname $0`

PAKET_BOOTSTRAPPER_EXE=.paket/paket.bootstrapper.exe
PAKET_EXE=.paket/paket.exe
FAKE_EXE=packages/FAKE/tools/FAKE.exe

FSIARGS=""
OS=${OS:-"unknown"}
if [[ "$OS" != "Windows_NT" ]]
then
  FSIARGS="--fsiargs -d:MONO"
fi

function run() {
  if [[ "$OS" != "Windows_NT" ]]
  then
    mono "$@"
  else
    "$@"
  fi
}

function yesno() {
  # NOTE: Defaults to NO
  read -p "$1 [y/N] " ynresult
  case "$ynresult" in
    [yY]*) true ;;
    *) false ;;
  esac
}

set +e
run $PAKET_BOOTSTRAPPER_EXE
bootstrapper_exitcode=$?
set -e

if [[ "$OS" != "Windows_NT" ]] &&
       [ $bootstrapper_exitcode -ne 0 ] &&
       [ $(certmgr -list -c Trust | grep X.509 | wc -l) -le 1 ] &&
       [ $(certmgr -list -c -m Trust | grep X.509 | wc -l) -le 1 ]
then
  echo "Your Mono installation has no trusted SSL root certificates set up."
  echo "This will result in the Paket bootstrapper failing to download Paket"
  echo "because Github's SSL certificate can't be verified. You can probably"
  echo "fix this by installing the latest mono-complete package, which will"
  echo "automatically sync certificates after installation. If that doesn't"
  echo "work and you are using mono >= 3.12.0 then you need to run:"
  echo ""
  echo "    cert-sync /path/to/certs"
  echo ""
  echo "For more information see:"
  echo ""
  echo "http://www.mono-project.com/docs/about-mono/releases/3.12.0/#cert-sync"
  echo ""
  echo "If you are using an older version of mono, you need to run:"
  echo ""
  echo "    mozroots --import --sync"
  exit 1
fi

run $PAKET_EXE restore

[ ! -e build.fsx ] && run $PAKET_EXE update
[ ! -e build.fsx ] && run $FAKE_EXE init.fsx
run $FAKE_EXE "$@" $FSIARGS build.fsx
