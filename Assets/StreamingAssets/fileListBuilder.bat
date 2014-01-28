
@echo off

setlocal EnableDelayedExpansion

del fileList.txt
del fileListExtract.txt

@rem this block runs 'dir /b /s', only displays files, trims off absolute path and only save the relative portion

set COMMAND_TO_RUN=dir /b /s /A-d /Ogn

set _t0=1
if ["%CD%"]==["%CD:~0,3%"] Set _t0=0
for /F "tokens=*" %%A In ('%COMMAND_TO_RUN%') Do (
   set _t1=%%A
   set _t2=!_t1:%CD%=!
   set _t3=!_t2:~%_t0%!
   set _t4=!_t3:\=/!
   echo !_t4!
   echo !_t4! >> fileList.txt
)


@rem do this again, but only set certain files as ones we want to actually extract to the temp folder

set COMMAND_TO_RUN=dir /b /s /A-d /Ogn *.bml

set _t0=1
if ["%CD%"]==["%CD:~0,3%"] Set _t0=0
for /F "tokens=*" %%A In ('%COMMAND_TO_RUN%') Do (
   set _t1=%%A
   set _t2=!_t1:%CD%=!
   set _t3=!_t2:~%_t0%!
   set _t4=!_t3:\=/!
   echo !_t4!
   echo !_t4! >> fileListExtract.txt
)

@echo on
