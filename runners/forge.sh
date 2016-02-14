#!/bin/bash

OS=${OS:-"unknown"}
function run() {
  if [[ "$OS" != "Windows_NT" ]]
  then
    mono "$@"
  else
    "$@"
  fi
}

run bin/forge.exe "$@"