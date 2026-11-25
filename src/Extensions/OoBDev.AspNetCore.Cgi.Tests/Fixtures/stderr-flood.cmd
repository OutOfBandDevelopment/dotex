@echo off
echo Content-Type: text/plain
echo.
echo ok
for /l %%i in (1,1,3000) do echo error line %%i 1>&2
