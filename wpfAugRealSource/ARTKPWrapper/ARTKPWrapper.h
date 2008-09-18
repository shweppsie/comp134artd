//----------------------------------------------
// (c) 2007 by casey chesnut, brains-N-brawn LLC
//----------------------------------------------
// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the ARTKPWRAPPER_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// ARTKPWRAPPER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef ARTKPWRAPPER_EXPORTS
#define ARTKPWRAPPER_API __declspec(dllexport)
#else
#define ARTKPWRAPPER_API __declspec(dllimport)
#endif

// This class is exported from the ARTKPWrapper.dll
class ARTKPWRAPPER_API CARTKPWrapper {
public:
	CARTKPWrapper(void);
	// TODO: add your methods here.
};

extern ARTKPWRAPPER_API int nARTKPWrapper;

ARTKPWRAPPER_API int fnARTKPWrapper(void);
