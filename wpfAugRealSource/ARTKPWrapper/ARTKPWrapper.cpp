//----------------------------------------------
// (c) 2007 by casey chesnut, brains-N-brawn LLC
//----------------------------------------------
// ARTKPWrapper.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "ARTKPWrapper.h"

#include "ARToolKitPlus/TrackerSingleMarkerImpl.h"
#include "ARToolKitPlus/TrackerMultiMarkerImpl.h"

#ifdef _MANAGED
#pragma managed(push, off)
#endif

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif

// This is an example of an exported variable
ARTKPWRAPPER_API int nARTKPWrapper=0;

// This is an example of an exported function.
ARTKPWRAPPER_API int fnARTKPWrapper(void)
{
	return 42;
}

class MyLogger : public ARToolKitPlus::Logger
{
    void artLog(const char* nStr)
    {
        printf(nStr);
    }
};

ARTKPWRAPPER_API int fnARTKPWrapperSingle(float* matrix, int& markerId, float& conf)
{
	// switch this between true and false to test
    // simple-id versus BCH-id markers
    //
    const bool    useBCH = false;

    const int     width = 320, height = 240, bpp = 1;
    size_t        numPixels = width*height*bpp;
    size_t        numBytesRead;
    const char    *fName = useBCH ? "data/image_320_240_8_marker_id_bch_nr0100.raw" :
                                      "data/image_320_240_8_marker_id_simple_nr031.raw";
    unsigned char *cameraBuffer = new unsigned char[numPixels];
    MyLogger      logger;

    // try to load a test camera image.
    // these images files are expected to be simple 8-bit raw pixel
    // data without any header. the images are expetected to have a
    // size of 320x240.
    //
    if(FILE* fp = fopen(fName, "rb"))
    {
        numBytesRead = fread(cameraBuffer, 1, numPixels, fp);
        fclose(fp);
    }
    else
    {
        printf("Failed to open %s\n", fName);
        delete cameraBuffer;
		return -1;
    }

    if(numBytesRead != numPixels)
    {
        printf("Failed to read %s\n", fName);
        delete cameraBuffer;
		return -2;
    }

    // create a tracker that does:
    //  - 6x6 sized marker images
    //  - samples at a maximum of 6x6
    //  - works with luminance (gray) images
    //  - can load a maximum of 1 pattern
    //  - can detect a maximum of 8 patterns in one image
    ARToolKitPlus::TrackerSingleMarker *tracker = new ARToolKitPlus::TrackerSingleMarkerImpl<6,6,6, 1, 8>(width,height);

	const char* description = tracker->getDescription();
	printf("ARToolKitPlus compile-time information:\n%s\n\n", description);

    // set a logger so we can output error messages
    //
    tracker->setLogger(&logger);
	tracker->setPixelFormat(ARToolKitPlus::PIXEL_FORMAT_LUM);
	//tracker->setLoadUndistLUT(true);

    // load a camera file. two types of camera files are supported:
    //  - Std. ARToolKit
    //  - MATLAB Camera Calibration Toolbox
    if(!tracker->init("data/LogitechPro4000.dat", 1.0f, 1000.0f))            // load std. ARToolKit camera file
    //if(!tracker->init("data/PGR_M12x0.5_2.5mm.cal", 1.0f, 1000.0f))        // load MATLAB file
	{
		printf("ERROR: init() failed\n");
		delete cameraBuffer;
		delete tracker;
		return -3;
	}

    // define size of the marker
    tracker->setPatternWidth(80);

	// the marker in the BCH test image has a thin border...
    tracker->setBorderWidth(useBCH ? 0.125f : 0.250f);

    // set a threshold. alternatively we could also activate automatic thresholding
    tracker->setThreshold(150);

    // let's use lookup-table undistortion for high-speed
    // note: LUT only works with images up to 1024x1024
    tracker->setUndistortionMode(ARToolKitPlus::UNDIST_LUT);

    // RPP is more robust than ARToolKit's standard pose estimator
    //tracker->setPoseEstimator(ARToolKitPlus::POSE_ESTIMATOR_RPP);

    // switch to simple ID based markers
    // use the tool in tools/IdPatGen to generate markers
    tracker->setMarkerMode(useBCH ? ARToolKitPlus::MARKER_ID_BCH : ARToolKitPlus::MARKER_ID_SIMPLE);


    // do the OpenGL camera setup
    //glMatrixMode(GL_PROJECTION)
    //glLoadMatrixf(tracker->getProjectionMatrix());

    // here we go, just one call to find the camera pose
    markerId = tracker->calc(cameraBuffer);
    conf = (float)tracker->getConfidence();

    // use the result of calc() to setup the OpenGL transformation
    //glMatrixMode(GL_MODELVIEW)
    //glLoadMatrixf(tracker->getModelViewMatrix());

	//matrix = new float[16];
    printf("\n\nFound marker %d  (confidence %d%%)\n\nPose-Matrix:\n  ", markerId, (int(conf*100.0f)));
	for(int i=0; i<16; i++)
	{
		printf("%.2f  %s", tracker->getModelViewMatrix()[i], (i%4==3)?"\n  " : "");
		float val = tracker->getModelViewMatrix()[i];
		matrix[i] = val;
	}

    delete [] cameraBuffer;
	delete tracker;

	return 0;
}

class LastCallLogger : public ARToolKitPlus::Logger
{
public:
	char* lastLog;

    void artLog(const char* nStr)
    {
        //printf(nStr);
		lastLog = (char *)nStr;
    }
};

/*
int _patternSizeX;
int _patternSizeY;
int _patternSampleNum;
int _maxLoadPatterns;
int _maxImagePatterns;
int _imageWidth;
int _imageHeight;
int _bpp = 1;
size_t _numPixels;
char * _fName;
unsigned char * _cameraBuffer;
LastCallLogger _logger;
*/

ARTKPWRAPPER_API int ARTKPLoadImagePath(char * fName, int imageWidth, int imageHeight, int bpp, unsigned char * cameraBuffer)
{
	size_t numPixels = imageWidth * imageHeight * bpp;
	//cameraBuffer = new unsigned char[numPixels];
	size_t numBytesRead;

    // try to load a test camera image.
    // these images files are expected to be simple 8-bit raw pixel
    // data without any header. the images are expetected to have a
    // size of 320x240.
    //
    if(FILE* fp = fopen(fName, "rb"))
    {
        numBytesRead = fread(cameraBuffer, 1, numPixels, fp);
        fclose(fp);
    }
    else
    {
        printf("Failed to open %s\n", fName);
        delete cameraBuffer;
        return -1;
    }

    if(numBytesRead != numPixels)
    {
        printf("Failed to read %s\n", fName);
        delete cameraBuffer;
        return -2;
    }
	return numBytesRead;
}

ARTKPWRAPPER_API int ARTKPLoadImageBytes(unsigned char * cameraBuffer, int bpp, unsigned char * outCameraBuffer)
{
	size_t numPixels = sizeof(cameraBuffer);
	//TODO outCameraBuffer is unnecessary
	return numPixels;
}

ARTKPWRAPPER_API ARToolKitPlus::TrackerSingleMarker* ARTKPConstructTrackerSingle(int trackerSwitch, int imageWidth, int imageHeight)
{
    //  - 6x6 sized marker images
    //  - samples at a maximum of 6x6
    //  - works with luminance (gray) images
    //  - can load a maximum of 1 pattern
    //  - can detect a maximum of 8 patterns in one image
	//
     //  __PATTERN_SIZE_X describes the pattern image width (16 by default).
	 //  __PATTERN_SIZE_Y describes the pattern image height (16 by default).
	 //  __PATTERN_SAMPLE_NUM describes the maximum resolution at which a pattern is sampled from the camera image
	 //  (64 by default, must a a multiple of __PATTERN_SIZE_X and __PATTERN_SIZE_Y).
	 //  __MAX_LOAD_PATTERNS describes the maximum number of pattern files that can be loaded.
	 //  __MAX_IMAGE_PATTERNS describes the maximum number of patterns that can be analyzed in a camera image.
	 //  Reduce __MAX_LOAD_PATTERNS and __MAX_IMAGE_PATTERNS to reduce memory footprint.

	//TODO change this so that i'm passing in a switch statement, and switching between which one is used
    ARToolKitPlus::TrackerSingleMarker *trackerSingle = new ARToolKitPlus::TrackerSingleMarkerImpl<6,6,6,1,8>(imageWidth, imageHeight);

	//NOTE how can i pass in template parameters at run-time? - cant, because this is done at compile time
	//ARToolKitPlus::TrackerSingleMarker *trackerSingle = new ARToolKitPlus::TrackerSingleMarkerImpl<patternSizeX, patternSizeY, patternSampleNum, maxLoadPatterns, maxImagePatterns>(imageWidth, imageHeight);

	//TODO get Logget back in
	//LastCallLogger logger;
	//trackerSingle->setLogger(&logger);
	return trackerSingle;
}

ARTKPWRAPPER_API ARToolKitPlus::TrackerMultiMarker* ARTKPConstructTrackerMulti(int trackerSwitch, int imageWidth, int imageHeight)
{
    // create a tracker that does:
    //  - 6x6 sized marker images
    //  - samples at a maximum of 6x6
    //  - works with luminance (gray) images
    //  - can load a maximum of 1 pattern
    //  - can detect a maximum of 16 patterns in one image
	//
	 //  __PATTERN_SIZE_X describes the pattern image width (16 by default).
	 //  __PATTERN_SIZE_Y describes the pattern image height (16 by default).
	 //  __PATTERN_SAMPLE_NUM describes the maximum resolution at which a pattern is sampled from the camera image
	 //  (64 by default, must a a multiple of __PATTERN_SIZE_X and __PATTERN_SIZE_Y).
	 //  __MAX_LOAD_PATTERNS describes the maximum number of pattern files that can be loaded.
	 //  __MAX_IMAGE_PATTERNS describes the maximum number of patterns that can be analyzed in a camera image.
	 //  Reduce __MAX_LOAD_PATTERNS and __MAX_IMAGE_PATTERNS to reduce memory footprint.

    ARToolKitPlus::TrackerMultiMarker *tracker = new ARToolKitPlus::TrackerMultiMarkerImpl<6,6,6, 1, 16>(imageWidth, imageHeight);
	return tracker;
}

ARTKPWRAPPER_API const char* ARTKPGetDescription(ARToolKitPlus::Tracker* tracker)
{
	const char* description = tracker->getDescription();
	char * desc = (char *) description;
	return description;
}

ARTKPWRAPPER_API char* ARTKPGetLastLog(ARToolKitPlus::Tracker* tracker)
{
	//TODO return _logger.lastLog;
	return "TODO";
}

ARTKPWRAPPER_API int ARTKPSetPixelFormat(ARToolKitPlus::Tracker* tracker, int pixelFormat)
{
	ARToolKitPlus::PIXEL_FORMAT pf = (ARToolKitPlus::PIXEL_FORMAT)pixelFormat; 
	tracker->setPixelFormat(pf);
	return (int) tracker->getPixelFormat();
}

ARTKPWRAPPER_API void ARTKPSetLoadUndistLUT(ARToolKitPlus::Tracker* tracker, bool value)
{
	tracker->setLoadUndistLUT(value);
}

ARTKPWRAPPER_API int ARTKPInit(ARToolKitPlus::TrackerSingleMarker* trackerSingle, char * camParamFile, float nearClip, float farClip)
{
	//if(!_trackerSingle->init("data/LogitechPro4000.dat", 1.0f, 1000.0f))            // load std. ARToolKit camera file
    //if(!tracker->init("data/PGR_M12x0.5_2.5mm.cal", 1.0f, 1000.0f))        // load MATLAB file
	if(!trackerSingle->init(camParamFile, nearClip, farClip))            // load std. ARToolKit camera file
	{
		printf("ERROR: init() failed\n");
		//delete _cameraBuffer;
		//delete _trackerSingle;
		return -3;
	}
	return 0;
}

ARTKPWRAPPER_API int ARTKPInitMulti(ARToolKitPlus::TrackerMultiMarker* trackerMulti, char * camParamFile, char * multiFile, float nearClip, float farClip)
{
	//if(!_trackerSingle->init("data/LogitechPro4000.dat", 1.0f, 1000.0f))            // load std. ARToolKit camera file
    //if(!tracker->init("data/PGR_M12x0.5_2.5mm.cal", 1.0f, 1000.0f))        // load MATLAB file
	if(!trackerMulti->init(camParamFile, multiFile, nearClip, farClip))            // load std. ARToolKit camera file
	{
		printf("ERROR: init() failed\n");
		//delete _cameraBuffer;
		//delete _trackerSingle;
		return -3;
	}
	return 0;
}

ARTKPWRAPPER_API void ARTKPSetPatternWidth(ARToolKitPlus::TrackerSingleMarker* trackerSingle, float value)
{
	// define size of the marker
    trackerSingle->setPatternWidth(value);
}

ARTKPWRAPPER_API void ARTKPSetBorderWidth(ARToolKitPlus::Tracker* tracker, float value)
{
	// the marker in the BCH test image has a thin border...
    tracker->setBorderWidth(value);
}

ARTKPWRAPPER_API void ARTKPSetThreshold(ARToolKitPlus::Tracker* tracker, float value)
{
    // set a threshold. alternatively we could also activate automatic thresholding
    tracker->setThreshold(value);
}

ARTKPWRAPPER_API int ARTKPGetThreshold(ARToolKitPlus::Tracker* tracker)
{
    return tracker->getThreshold();
}

ARTKPWRAPPER_API void ARTKPSetUndistortionMode(ARToolKitPlus::Tracker* tracker, int value)
{
    // let's use lookup-table undistortion for high-speed
    // note: LUT only works with images up to 1024x1024
	ARToolKitPlus::UNDIST_MODE u = (ARToolKitPlus::UNDIST_MODE) value;
    tracker->setUndistortionMode(u);
}

ARTKPWRAPPER_API void ARTKPSetPoseEstimator(ARToolKitPlus::Tracker* tracker, int value)
{
    // RPP is more robust than ARToolKit's standard pose estimator
	ARToolKitPlus::POSE_ESTIMATOR pe = (ARToolKitPlus::POSE_ESTIMATOR) value;
    tracker->setPoseEstimator(pe);
}

ARTKPWRAPPER_API void ARTKPSetMarkerMode(ARToolKitPlus::Tracker* tracker, int markerId)
{
    // switch to simple ID based markers
    // use the tool in tools/IdPatGen to generate markers
	ARToolKitPlus::MARKER_MODE mi = (ARToolKitPlus::MARKER_MODE) markerId;
    tracker->setMarkerMode(mi);
}

ARTKPWRAPPER_API float ARTKPGetConfidence(ARToolKitPlus::TrackerSingleMarker* trackerSingle)
{
    float conf = (float)trackerSingle->getConfidence();
	return conf;
}

ARTKPWRAPPER_API void ARTKPGetModelViewMatrix(ARToolKitPlus::Tracker* tracker, float* matrix)
{
	//for(int i=0; i<16; i++)
	//	printf("%.2f  %s", tracker->getModelViewMatrix()[i], (i%4==3)?"\n  " : "");
	//float * retVal = new float[16];
	for(int i=0; i<16; i++)
	{
		float val = tracker->getModelViewMatrix()[i];
		matrix[i] = val;
	}
	//return retVal;
}

ARTKPWRAPPER_API void ARTKPGetProjectionMatrix(ARToolKitPlus::Tracker* tracker, float* matrix)
{
	for(int i=0; i<16; i++)
	{
		float val = tracker->getProjectionMatrix()[i];
		matrix[i] = val;
	}
}

/*
ARTKPWRAPPER_API int ARTKPCalc(ARToolKitPlus::Tracker* tracker, unsigned char* cameraBuffer)
{
	int markerId = tracker->calc(cameraBuffer);
	return markerId;
}
*/

ARTKPWRAPPER_API int ARTKPCalc(ARToolKitPlus::TrackerSingleMarker* trackerSingle, unsigned char* cameraBuffer, int pattern, bool updateMatrix, ARToolKitPlus::ARMarkerInfo** markerInfos, int& numMarkers)
{
	int markerId = trackerSingle->calc(cameraBuffer, pattern, updateMatrix, markerInfos, &numMarkers);
	return markerId;
}

ARTKPWRAPPER_API int ARTKPCalcMulti(ARToolKitPlus::TrackerMultiMarker* trackerMulti, unsigned char* cameraBuffer)
{
	int numMarkers = trackerMulti->calc(cameraBuffer);
	return numMarkers;
}

ARTKPWRAPPER_API int ARTKPDetectMarker(ARToolKitPlus::Tracker* tracker, unsigned char* cameraBuffer, int thresh, ARToolKitPlus::ARMarkerInfo* markerInfos, int numMarkers)
{
	//UNTESTED
	int retVal = tracker->arDetectMarker(cameraBuffer, thresh, &markerInfos, &numMarkers);
	return retVal;
}

ARTKPWRAPPER_API int ARTKPDetectMarkerLite(ARToolKitPlus::Tracker* tracker, unsigned char* cameraBuffer, int thresh, ARToolKitPlus::ARMarkerInfo* markerInfos, int numMarkers)
{
	//UNTESTED
	int retVal = tracker->arDetectMarkerLite(cameraBuffer, thresh, &markerInfos, &numMarkers);
	return retVal;
}

ARTKPWRAPPER_API float ARTKPGetTransMat(ARToolKitPlus::Tracker* tracker, ARToolKitPlus::ARMarkerInfo* markerInfo, float* center, float width, float* matrix)
{
	ARFloat conv[3][4];
	float retVal = tracker->arGetTransMat(markerInfo, center, width, conv);
	for(int i=0; i<3; i++)
	{
		for(int j=0; j<4; j++)
		{
			int x = (i * 4) + j;
			matrix[x] = conv[i][j];
		}
	}
	return retVal;
}

ARTKPWRAPPER_API float ARTKPGetTransMatCont(ARToolKitPlus::Tracker* tracker, ARToolKitPlus::ARMarkerInfo* markerInfo, ARFloat preConv[][4], float* center, float width, float* matrix)
{
	//UNTESTED
	ARFloat conv[3][4];
	float retVal = tracker->arGetTransMatCont(markerInfo, preConv, center, width, conv);
	for(int i=0; i<3; i++)
	{
		for(int j=0; j<4; j++)
		{
			int x = (i * 4) + j;
			matrix[x] = conv[i][j];
		}
	}
	return retVal;
}

ARTKPWRAPPER_API void ARTKPCleanup(ARToolKitPlus::Tracker* tracker, unsigned char * cameraBuffer)
{
	delete [] cameraBuffer;
	delete tracker;
}

ARTKPWRAPPER_API const ARToolKitPlus::ARMultiMarkerInfoT* ARTKPGetMultiMarkerConfig(ARToolKitPlus::TrackerMultiMarker* trackerMulti)
{
	const ARToolKitPlus::ARMultiMarkerInfoT* config = trackerMulti->getMultiMarkerConfig();
	return config;
}

ARTKPWRAPPER_API void ARTKPGetDetectedMarkers(ARToolKitPlus::TrackerMultiMarker* trackerMulti, int*& markerIDs)
{
	//UNTESTED
	trackerMulti->getDetectedMarkers(markerIDs);
}

ARTKPWRAPPER_API const ARToolKitPlus::ARMarkerInfo& ARTKPGetDetectedMarker(ARToolKitPlus::TrackerMultiMarker* trackerMulti, int marker)
{
	const ARToolKitPlus::ARMarkerInfo& markerInfo = trackerMulti->getDetectedMarker(marker);
	return markerInfo;
}

ARTKPWRAPPER_API ARToolKitPlus::ARMarkerInfo ARTKPGetDetectedMarkerStruct(ARToolKitPlus::TrackerMultiMarker* trackerMulti, int marker)
{
	return trackerMulti->getDetectedMarker(marker);
}

ARTKPWRAPPER_API float ARTKPArMultiGetTransMat(ARToolKitPlus::TrackerMultiMarker* trackerMulti, ARToolKitPlus::ARMarkerInfo* markerInfo, int markerNum, ARToolKitPlus::ARMultiMarkerInfoT* config)
{
	return trackerMulti->arMultiGetTransMat(markerInfo, markerNum, config);
}

ARTKPWRAPPER_API int ARTKPGetNumDetectedMarkers(ARToolKitPlus::TrackerMultiMarker* trackerMulti)
{
	//UNTESTED
	return trackerMulti->getNumDetectedMarkers();
}

ARTKPWRAPPER_API void ARTKPSetUseDetectLite(ARToolKitPlus::TrackerMultiMarker* trackerMulti, bool enable)
{
	//UNTESTED
	trackerMulti->setUseDetectLite(enable);
}

ARTKPWRAPPER_API void ARTKPGetARMatrixMulti(ARToolKitPlus::TrackerMultiMarker* trackerMulti, float* matrix)
{
	//UNTESTED
	ARFloat conv[3][4];
	trackerMulti->getARMatrix(conv);
	for(int i=0; i<3; i++)
	{
		for(int j=0; j<4; j++)
		{
			int x = (i * 4) + j;
			matrix[x] = conv[i][j];
		}
	}
}

ARTKPWRAPPER_API void ARTKPGetARMatrix(ARToolKitPlus::TrackerSingleMarker* trackerSingle, float* matrix)
{
	//UNTESTED
	ARFloat conv[3][4];
	trackerSingle->getARMatrix(conv);
	for(int i=0; i<3; i++)
	{
		for(int j=0; j<4; j++)
		{
			int x = (i * 4) + j;
			matrix[x] = conv[i][j];
		}
	}
}

ARTKPWRAPPER_API int ARTKPAddPattern(ARToolKitPlus::TrackerSingleMarker* trackerSingle, char * fName)
{
	//UNTESTED
	return trackerSingle->addPattern(fName);
}

ARTKPWRAPPER_API int ARTKPArDetectMarker(ARToolKitPlus::Tracker* tracker, unsigned char* cameraBuffer, int thresh, ARToolKitPlus::ARMarkerInfo** markerInfos, int& numMarkers)
{
	return tracker->arDetectMarker(cameraBuffer, thresh, markerInfos, &numMarkers);
}
 
ARTKPWRAPPER_API int ARTKPArDetectMarkerLite(ARToolKitPlus::Tracker* tracker, unsigned char* cameraBuffer, int thresh, ARToolKitPlus::ARMarkerInfo** markerInfos, int& numMarkers)
{
	return tracker->arDetectMarkerLite(cameraBuffer, thresh, markerInfos, &numMarkers);
}

ARTKPWRAPPER_API void ARTKPActivateBinaryMarker(ARToolKitPlus::Tracker* tracker, int threshold)
{
	tracker->activateBinaryMarker(threshold);
}

ARTKPWRAPPER_API void ARTKPActivateVignettingCompensation(ARToolKitPlus::Tracker* tracker, bool nEnable, int nCorners, int nLeftRight, int nTopBottom)
{
	tracker->activateVignettingCompensation(nEnable, nCorners, nLeftRight, nTopBottom);
}

ARTKPWRAPPER_API int ARTKPGetNumLoadablePatterns(ARToolKitPlus::Tracker* tracker)
{
	return tracker->getNumLoadablePatterns();
}

ARTKPWRAPPER_API bool ARTKPLoadCameraFile(ARToolKitPlus::Tracker* tracker, const char* nCamParamFile, float nNearClip, float nFarClip)
{
	return tracker->loadCameraFile(nCamParamFile, nNearClip, nFarClip);
}

ARTKPWRAPPER_API void ARTKPSetImageProcessingMode(ARToolKitPlus::Tracker* tracker, int imageProcMode)
{
	ARToolKitPlus::IMAGE_PROC_MODE ipm = (ARToolKitPlus::IMAGE_PROC_MODE) imageProcMode;
	tracker->setImageProcessingMode(ipm);
}

ARTKPWRAPPER_API void ARTKPSetNumAutoThresholdRetries(ARToolKitPlus::Tracker* tracker, int numRetries)
{
	tracker->setNumAutoThresholdRetries(numRetries);
}

ARTKPWRAPPER_API bool ARTKPIsAutoThresholdActivated(ARToolKitPlus::Tracker* tracker)
{
	return tracker->isAutoThresholdActivated();
}

ARTKPWRAPPER_API void ARTKPActivateAutoThreshold(ARToolKitPlus::Tracker* tracker, bool enable)
{
	tracker->activateAutoThreshold(enable);
}

ARTKPWRAPPER_API float ARTKPExecuteSingleMarkerPoseEstimator(ARToolKitPlus::Tracker* tracker, ARToolKitPlus::ARMarkerInfo *marker_info, float* center, float width, float* matrix)
{
	ARFloat conv[3][4];
	float retVal = tracker->executeSingleMarkerPoseEstimator(marker_info, center, width, conv);
	for(int i=0; i<3; i++)
	{
		for(int j=0; j<4; j++)
		{
			int x = (i * 4) + j;
			matrix[x] = conv[i][j];
		}
	}
	return retVal;
}

ARTKPWRAPPER_API float ARTKPExecuteMultiMarkerPoseEstimator(ARToolKitPlus::Tracker* tracker, ARToolKitPlus::ARMarkerInfo *marker_info, int marker_num, ARToolKitPlus::ARMultiMarkerInfoT *config)
{
	return tracker->executeMultiMarkerPoseEstimator(marker_info, marker_num, config);
}

ARTKPWRAPPER_API int fnARTKPWrapperMulti(void)
{
const int     width = 320, height = 240, bpp = 1;
    size_t        numPixels = width*height*bpp;
    size_t        numBytesRead;
    const char    *fName = "data/markerboard_480-499.raw";
    unsigned char *cameraBuffer = new unsigned char[numPixels];
    MyLogger      logger;

    // try to load a test camera image.
    // these images files are expected to be simple 8-bit raw pixel
    // data without any header. the images are expetected to have a
    // size of 320x240.
    //
    if(FILE* fp = fopen(fName, "rb"))
    {
        numBytesRead = fread(cameraBuffer, 1, numPixels, fp);
        fclose(fp);
    }
    else
    {
        printf("Failed to open %s\n", fName);
        delete cameraBuffer;
        return -1;
    }

    if(numBytesRead != numPixels)
    {
        printf("Failed to read %s\n", fName);
        delete cameraBuffer;
        return -2;
    }

    // create a tracker that does:
    //  - 6x6 sized marker images
    //  - samples at a maximum of 6x6
    //  - works with luminance (gray) images
    //  - can load a maximum of 1 pattern
    //  - can detect a maximum of 8 patterns in one image
    ARToolKitPlus::TrackerMultiMarker *tracker = new ARToolKitPlus::TrackerMultiMarkerImpl<6,6,6, 1, 16>(width,height);

	const char* description = tracker->getDescription();
	printf("ARToolKitPlus compile-time information:\n%s\n\n", description);

    // set a logger so we can output error messages
    //
    tracker->setLogger(&logger);
	tracker->setPixelFormat(ARToolKitPlus::PIXEL_FORMAT_LUM);

    // load a camera file. two types of camera files are supported:
    //  - Std. ARToolKit
    //  - MATLAB Camera Calibration Toolbox
	if(!tracker->init("data/LogitechPro4000.dat", "data/markerboard_480-499.cfg", 1.0f, 1000.0f))
	{
		printf("ERROR: init() failed\n");
		delete cameraBuffer;
		delete tracker;
		return -3;
	}

	// the marker in the BCH test image has a thiner border...
    tracker->setBorderWidth(0.125f);

    // set a threshold. we could also activate automatic thresholding
    tracker->setThreshold(160);

    // let's use lookup-table undistortion for high-speed
    // note: LUT only works with images up to 1024x1024
    tracker->setUndistortionMode(ARToolKitPlus::UNDIST_LUT);

    // RPP is more robust than ARToolKit's standard pose estimator
    //tracker->setPoseEstimator(ARToolKitPlus::POSE_ESTIMATOR_RPP);

    // switch to simple ID based markers
    // use the tool in tools/IdPatGen to generate markers
    tracker->setMarkerMode(ARToolKitPlus::MARKER_ID_SIMPLE);

    // do the OpenGL camera setup
    //glMatrixMode(GL_PROJECTION)
    //glLoadMatrixf(tracker->getProjectionMatrix());

    // here we go, just one call to find the camera pose
    int numDetected = tracker->calc(cameraBuffer);

    // use the result of calc() to setup the OpenGL transformation
    //glMatrixMode(GL_MODELVIEW)
    //glLoadMatrixf(tracker->getModelViewMatrix());

	printf("\n%d good Markers found and used for pose estimation.\nPose-Matrix:\n  ", numDetected);
	for(int i=0; i<16; i++)
		printf("%.2f  %s", tracker->getModelViewMatrix()[i], (i%4==3)?"\n  " : "");

	bool showConfig = false;

	if(showConfig)
	{
		const ARToolKitPlus::ARMultiMarkerInfoT *artkpConfig = tracker->getMultiMarkerConfig();
		printf("%d markers defined in multi marker cfg\n", artkpConfig->marker_num);

		printf("marker matrices:\n");
		for(int multiMarkerCounter = 0; multiMarkerCounter < artkpConfig->marker_num; multiMarkerCounter++)
		{
			printf("marker %d, id %d:\n", multiMarkerCounter, artkpConfig->marker[multiMarkerCounter].patt_id);
			for(int row = 0; row < 3; row++)
			{
				for(int column = 0; column < 4; column++)
					printf("%.2f  ", artkpConfig->marker[multiMarkerCounter].trans[row][column]);
				printf("\n");
			}
		}
	}

    delete [] cameraBuffer;
	delete tracker;
	return numDetected; //0
}

// This is the constructor of a class that has been exported.
// see ARTKPWrapper.h for the class definition
CARTKPWrapper::CARTKPWrapper()
{
	return;
}
