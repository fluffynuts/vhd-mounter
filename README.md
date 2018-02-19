# VHDMounter
-----------

## What
-------
A very simple VHD mounting service for windows.

## Why
------
My specific usage is for MySql on a GPT partition (which MySql, at time of writing, does not approve of, but I have little option here since it's a bootcamp partition on a mac). Turns out you can get MySql working fine on a mounted VHD -- which I have. This service just makes usage of the VHD simpler.

## Usage
--------
1. Build binaries & copy wherever you please
2. Create an ini file in the folder where the app resides, called "vhdmounter.ini"
3. Add one section per disk you'd like to mount -- the name is only useful for you, but you do need to specify the `path` setting, which is a fully-qualified path to the vhd you'd like to mount.
4. Run: `vhdmounter.exe -i` to install
5. Optionaly make other services depend on the mounter
    - check their dependencies first with `sc qc {service}`
    - update their dependencies with `sc config {service} depend= vhdmounter/{original dep1}/{original dep2}`
    - note that `sc config` overwrites the existing dependency list, so you *must* configure the original dependencies into that list, if there are any. Also note the space after `depend=` and not before the `=`. More here: [https://serverfault.com/questions/24821/how-to-add-dependency-on-a-windows-service-after-the-service-is-installed#228326](https://serverfault.com/questions/24821/how-to-add-dependency-on-a-windows-service-after-the-service-is-installed#228326)
6. `net start vhdmounter` should mount your disk(s). Alternatively, if you have any service dependencies set up (5 above), then `net start {your service}` should mount disks and start your service.
