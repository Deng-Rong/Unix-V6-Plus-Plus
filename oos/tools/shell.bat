@pushd .
@cd /d "%oos_path%\src\lib" && make
@popd

@pushd .
@cd /d "%oos_path%\src\shell" && make
@popd