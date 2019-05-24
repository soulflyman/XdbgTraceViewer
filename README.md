# XdbgTraceViewer
Display Xdebug generated trace files.

![Screenshot of XdbgTraceViewer](https://github.com/soulflyman/XdbgTraceViewer/blob/master/XdbgTraceViewer_screenshot.png)

# Requirements
## Windows
[.Net Framework 4.7](https://www.microsoft.com/en-us/download/details.aspx?id=55170) is required to run this Application. 

## Webserver
Install Xdebug and test if you can Debug some PHP files. After that set the following parameters in your php.ini:
```
xdebug.trace_options=0
xdebug.trace_format=1
```
Now you can start the trace by setting the cookie for xdebug. `` xdebug.trace_options=0``  is not required but recomended.

**OR**

Use the Xdebug php functions [xdebug_start_trace()](https://xdebug.org/docs/execution_trace#xdebug_start_trace) and xdebug_stop_trace() in your code to generate a trace file.

# Note
I created this tool only to finde a error in a 2gb sized trace file (I know I know...).   
So use it at your own risk. It is not optimized and can crash at every moment.
