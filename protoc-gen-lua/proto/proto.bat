
for %%i in (*.proto) do protoc.exe --cpp_out=./ %%i

cd "res"
for %%i in (*.proto) do protoc.exe --cpp_out=./ %%i
pause
