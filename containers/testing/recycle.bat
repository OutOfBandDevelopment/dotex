  cd C:\repo\oobdev\dotex\containers\testing
  CALL scripts\integration-down.bat --clean
  CALL scripts\integration-up.bat --wait
  START http://localhost:8080/