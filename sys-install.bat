@rem  module:  sys-install.bat              syswin auto installer
@rem project:  manufacture syswin           syswin.manufacture.net      
@rem author:   Francis Korning              <fkorning@yahoo.ca>
@rem rights:   (c) 2014 Francis Korning     manufacture.net
@rem license:   GNU LGPL license            https://www.gnu.org/licenses/lgpl.html


@echo off

if "%1"=="/?" goto USAGE
if "%1"=="/h" goto USAGE
if "%1"=="-h" goto USAGE


:INSTALL
@rem su --login --mimic --elev --user administrator --cmd "sys-setup.bat^"
runas /user:%COMPUTERNAME%\administrator "cmd /k ^cd /d %CD% && sys-setup.bat^"


GOTO END


:USAGE
echo "usage:  sys-install"
goto END


:END
echo done.