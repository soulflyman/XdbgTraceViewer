# XdbgTraceViewer
Display Xdebug generated trace files.

# Requirements
Install Xdebug and test if you can Debug some PHP files. After that set the following parameters in your php.ini:
```
xdebug.trace_options=0
xdebug.trace_format=1
```

**xdebug.trace_options=0** is not required but recomended.

# Note
I created this tool only to finde a error in a 2gb sized trace file (I know I know...).
So use it as your own risk. It is not optimiced and can crash at every moment.
