@rem  module:  sys-setup.bat                syswin configurator
@rem project:  manufacture syswin           syswin.manufacture.net      
@rem author:   Francis Korning              <fkorning@yahoo.ca>
@rem rights:   (c) 2014 Francis Korning     manufacture.net
@rem license:   GNU LGPL license            https://www.gnu.org/licenses/lgpl.html


@rem echo off

@echo NOTE: do not forget to set the password for the administrator!
@echo NOTE: do not forget to set the password for user cyg_server!
@echo NOTE: do not forget to set the password for mysql root!


:DIRS
@echo checking for manufacture syswin
if exist "c:/syswin/bin" goto CHECK


:FETCH
@echo fetching sysinternals suite zip
if exist sys-internals.zip goto BINS
./bin/httpget -u https://download.sysinternals.com/files/SysinternalsSuite.zip -d  c:/users/public/downloads -f sys-internals.zip


:BINS
@echo installing manufacture syswin binaries
md "c:/syswin"
md "c:/syswin/bin"
cp ./bin/*.* c:/sywin/bin
./bin/unzip -d c:/syswin/bin c:/users/public/downloads/sys-internals.zip


:EULA
@echo activating sysinternals EULA licenses
c:/syswin/bin/junction.exe -accapteula


:CHECK
@echo checking for manufacture syswin sysinternals junction
if exist "c:/syswin/bin/junction.exe" goto SYSWIN
@echo ERROR: manufacture syswin is missing sysinternals junction. 
goto END


:SYSWIN
@echo SUCCESS manufacture syswin junction found. continuing install.


:ENVS
@echo installing manufacture syswin

@set SYSWIN=c:/syswin
@setx SYSWIN c:/syswin
@setx /m SYSWIN c:/syswin

@set PATH=c:/syswin;c:/syswin/bin;%PATH%
@rem todo: gtools pathed: add path to system path
@rem setx /m PATH c:/syswin;c:/syswin/bin;%PATH%


:CONFIG
@echo configuring manufacture syswin
set DIR=%CD%
@cd /


:DIRS:
@echo configuring cygwin dirs
@if not exist "c:/cygwin" md "c:/cygwin"
@if not exist "c:/cygdrive" md "c:/cygdrive"

@echo configuring syswin dirs
@if not exist "c:/syswin" md "c:/syswin"
@if not exist "c:/vault" md "c:/vault"
@if not exist "c:/work" md "c:/work"
@if not exist "c:/mnt" md "c:/mnt"
@if not exist "c:/share" md "c:/share"
@if not exist "c:/share/lib" md "c:/share/lib"


:LINKS:
@echo configuring cygwin junction links
@if not exist "c:/cygwin/windows" junction "c:/cygwin/windows" "c:/windows"
@if not exist "c:/cygwin/cygwin" junction "c:/cygwin/cygwin" "c:/cygwin"
@if not exist "c:/cygwin/syswin" junction "c:/cygwin/syswin" "c:/syswin"
@if not exist "c:/cygwin/share" junction "c:/cygwin/share" "c:/share"
@if not exist "c:/cygwin/vault" junction "c:/cygwin/vault" "c:/vault"
@if not exist "c:/cygwin/users" junction "c:/cygwin/users" "c:/users"
@if not exist "c:/cygwin/work" junction "c:/cygwin/work" "c:/work"
@if not exist "c:/cygwin/mnt" junction "c:/cygwin/mnt" "c:/mnt"

@echo configuring syswin junction links
@if not exist "c:/bin" junction "c:/bin" "c:/cygwin/bin"
@if not exist "c:/dev" junction "c:/dev" "c:/cygwin/dev"
@if not exist "c:/lib" junction "c:/lib" "c:/cygwin/lib"
@if not exist "c:/etc" junction "c:/etc" "c:/cygwin/etc"
@if not exist "c:/usr" junction "c:/usr" "c:/cygwin/usr"
@if not exist "c:/tmp" junction "c:/tmp" "c:/cygwin/tmp"
@if not exist "c:/var" junction "c:/var" "c:/cygwin/var"
@if not exist "c:/sbin" junction "c:/sbin" "c:/cygwin/sbin"
@if not exist "c:/proc" junction "c:/proc" "c:/cygwin/proc"
@if not exist "c:/home" junction "c:/home" "c:/cygwin/home"

@if not exist "c:/cygdrive/c" junction "c:/cygdrive/c" "c:/"
@if not exist "c:/users/All" junction "c:/users/All" "c:/users/All Users"
#@if not exist "c:/users/Default" junction "c:/users/Default" "c:/users/Default User"
#@if not exist "c:/users/Public User" junction "c:/users/Public User" "c:/users/Default"

@if not exist "c:/share/lib/data" junction "c:/share/lib/data" "c:/ProgramData"
@if not exist "c:/share/lib/programs" junction "c:/share/lib/programs" "c:/Program Files"
@if not exist "c:/share/lib/programs_x64" junction "c:/share/lib/programs_x64" "c:/Program Files"
@if not exist "c:/share/lib/programs_x86" junction "c:/share/lib/programs_x86" "c:/Program Files (X86)"


cd %DIR%
@echo manufacture syswin configured


:END
@echo done.
