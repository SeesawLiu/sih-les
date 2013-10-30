@echo off
set DbHost=10.86.13.83
set DbName=sconit5_2nd_test
set DbUser=sa
set DbPass=P@ssw0rd
for /f %%i in ('dir/b *.sql') do (
osql -S "%DbHost%" -d "%DbName%" -U "%DbUser%" -P "%DbPass%" -i "%%i" -o "%%~ni.log"
)
pause


