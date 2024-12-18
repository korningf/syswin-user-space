```

#=============================================================================#
# Manufakture syswin-user-space
#-----------------------------------------------------------------------------#

                              _____        __      __                        
  _____ _____    ____  __ ___/ ____\____  |  | ___/  |_ __ _________   ____  
 /     \\__  \  /    \|  |  \   __\\__  \ |  |/ /\   __\  |  \_  __ \_/ __ \ 
|  Y Y  \/ __ \|   |  \  |  /|  |   / __ \|    <  |  | |  |  /|  | \/\  ___/ 
|__|_|  (____  /___|  /____/ |__|  (____  /__|_ \ |__| |____/ |__|    \___  >
      \/     \/     \/                  \/     \/                         \/

#-----------------------------------------------------------------------------#

#=============================================================================#

```	
	

Manufakture syswin utilities in user-space on secondary drive (L:)


[TOC]


#-----------------------------------------------------------------------------#
# Manufakture syswin-user-space
#-----------------------------------------------------------------------------#

	Syswin is a collection of Windows bootstrap utilities and binaries that serve
	as prererquisites for our POSIX windows platform.  It is based on the excellent
	SysInternals Suite by Marc Russinovitch, with added binaries for fetching and
	unpacking files, shell path and environment automation, and a super-user shell
	wrapper to expose and assume administrator privilege and launch root commands.
	

	Manufacture Syswin installs almost programatically from within an admin shell.


#-----------------------------------------------------------------------------#
# Capabilities:
#-----------------------------------------------------------------------------#


	Capabilities:
		
		- POSIX unified directory file structure
		- sysinternals binaries and applications
		- junction, mklink, psexec, pskill, pslist, psfile, pspasswd
		- zip/unzip, gzip/gunzip, bzip/bunzip
		- httpget, pathed, su


#-----------------------------------------------------------------------------#
# POSIX Structure:
#-----------------------------------------------------------------------------#

	POSIX Structure:

		/windows/		windows installation
		/cygwin/		cygwin binaries
		/syswin/		syswin binaries

		/				main cygwin mountpoint 
		/mnt/			additional mountpoints
		cygdrive/		mounted windows drives

		dev/			devices (virtual)
		proc/			processes (virtual)

		var/			var files (permanent)
		tmp/			tmp files (volatile)
		
		etc/			configuration files
		lib/			runtime libraries

		usr/			user programs
		bin/			user binaries
		sbin/			admin binaries	
		setup/			setup directory
		
		home/			posix user profiles
		users/			windows user profiles
		
		vault/			private passwd vault
		work/			work apps and workspaces
		
		share/			shared data, libs, programs 
		srv/			shared public data, servers
	

#-----------------------------------------------------------------------------#
# Requirements:
#-----------------------------------------------------------------------------#

	Requirements:

    - windows 10/11  x86 or x64(recommended) 
    
    - windows user with local administrator rights
    
    - internet / proxy access and zip/unzip archiver 
    
    - space for cygwin in the default drive c:\cygwin
    
    - space for syswin in the default drive c:\syswin
    
    
	Prerequisites:

    - start by exposing the real administrator account

        runas /user:%COMPUTERNAME%\administrator "cmd -c ^net user administrator /active:yes^"

    - login as Administrator and set the password (memorize it!)
    
        runas /user:%COMPUTERNAME%\administrator "cmd -k"
        
        net user administrator <password>
        
   

#-----------------------------------------------------------------------------#
# Installation:
#-----------------------------------------------------------------------------#

	Installation:
        
    - from an interactive admininstrator shell run sys-install.bat

		sys-install.bat

	


#-----------------------------------------------------------------------------#
# Configuration:
#-----------------------------------------------------------------------------#



#-----------------------------------------------------------------------------#
# Operation:
#-----------------------------------------------------------------------------#



#-----------------------------------------------------------------------------#
# LICENSE
#-----------------------------------------------------------------------------#


  [LICENSE.txt](LICENSE.txt)

```   

#-----------------------------------------------------------------------------#
# (c) Francis Korning 2024.
#=============================================================================#

```
