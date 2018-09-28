# qml-md
QML interface for doing MD in VR. 

## Prerequisites for Windows: 
* Visual Studio 2017+, with the following additional options:
  - .NET Desktop development: .NET Framework 4 - 4.6 development tools, .NET Framework 4.6.2 development tools. 
  - Desktop development with C++: Just-In-Time debugger, VC++ 2017 version 15.7+, VC++ 2015.3+
  - Python development: Python native development tools.  
* A fortran compiler for QML, [Intel Parallel Studio XE](https://software.intel.com/en-us/parallel-studio-xe) have academic licenses.
* A python 3.5+ installation with pythonnet. Anaconda recommended, then install pythonnet to an environment:
```
conda create --name=env pytohn=3.5 
conda activate net 
pip install pythonnet
```

### Notes 
Assumes pythonnet is installed with anaconda (currently hardcoded, working with Windows).

Assumes VR is distributed via the itch.io client. Check the Simbox.QML.csproj file to ensure references are set correctly. 
This file also includes an automatic build script to install the plugin into the itch.io release. 
