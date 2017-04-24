#!/usr/bin/env bash

mono=
fsiargs=()

if [[ "$OS" != "Windows_NT" ]]; then
  mono=mono
  fsiargs=(--fsiargs -d:MONO)

  # http://fsharp.github.io/FAKE/watch.html
  export MONO_MANAGED_WATCHER=false
fi

$mono .paket/paket.exe restore || exit $?
$mono packages/FAKE/tools/FAKE.exe "$@" "${fsiargs[@]}" build.fsx
