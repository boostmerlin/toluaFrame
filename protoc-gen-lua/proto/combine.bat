del client.proto
echo syntax = "proto2";  >> client.tmp
echo package client; >> client.tmp
echo. >> client.tmp
for %%i in (*.proto) do type %%i >> client.tmp && echo. >> client.tmp && echo. >> client.tmp

findstr /v /b "import" client.tmp > client2.proto
del client.tmp
ren client2.proto client.proto

pause
