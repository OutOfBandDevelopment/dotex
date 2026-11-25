#!/bin/sh
echo "Content-Type: text/plain"
echo
echo "ok"
i=0
while [ $i -lt 3000 ]; do echo "error line $i" 1>&2; i=$((i+1)); done
