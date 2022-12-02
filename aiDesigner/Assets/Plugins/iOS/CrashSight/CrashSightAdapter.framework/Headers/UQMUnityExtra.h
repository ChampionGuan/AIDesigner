//
// Created by joyfyzhang on 2021/10/19.
//

#ifndef ANDROID_UQMUNITYEXTRA_H
#define ANDROID_UQMUNITYEXTRA_H

#include "UQMUnityBridge.h"

extern "C"
{
    ///----- UQMCrash
    void UQM_EXPORT cs_configCallbackTypeAdapter(int32_t callbackType);

    void UQM_EXPORT cs_configGameTypeAdapter(int gameType);

    void UQM_EXPORT cs_configAutoReportLogLevelAdapter(int level);

    void UQM_EXPORT cs_configDefaultAdapter(const char *channel, const char *version, const char *user, int64_t delay);

    void UQM_EXPORT cs_configCrashServerUrlAdapter(const char *serverUrl);

    void UQM_EXPORT cs_configDebugModeAdapter(bool enable);

    void UQM_EXPORT cs_setDeviceIdAdapter(const char *deviceId);

    void UQM_EXPORT cs_setDeviceModelAdapter(const char *deviceModel);

    void UQM_EXPORT cs_initWithAppIdAdapter(const char *appId);

    void UQM_EXPORT cs_logRecordAdapter(int level, const char *message);

    void UQM_EXPORT cs_addSceneDataAdapter(const char *key, const char *value);

    // extras only for ios
    void UQM_EXPORT cs_reportExceptionV1Adapter(int type, const char *name, const char *message, const char *stackTrace, const char *extras, bool quitProgram);

    // new add api
    void UQM_EXPORT cs_reportExceptionV2Adapter(int type, const char *exceptionName, const char *exceptionMsg, const char *exceptionStack, const char *paramsJson, int dumpNativeType);

    void UQM_EXPORT cs_setUserIdAdapter(const char *userId);

    void UQM_EXPORT cs_setSceneAdapter(int sceneId);

    void UQM_EXPORT cs_setLogPathAdapter(const char *logPath);

    void UQM_EXPORT cs_reRegistAllMonitorsAdapter();

    void UQM_EXPORT cs_setAppVersionAdapter(const char *appVersion);

    void UQM_EXPORT cs_crashObserverAdapter();

    void UQM_EXPORT cs_crashLogObserverAdapter();

    void UQM_EXPORT cs_reportLogInfo(const char *msgType, const char *msg);

    // test api
    void UQM_EXPORT cs_testOomCrashAdapter();

    void UQM_EXPORT cs_testJavaCrashAdapter();

    void UQM_EXPORT cs_testOcCrashAdapter();

    void UQM_EXPORT cs_testNativeCrashAdapter();

    void UQM_EXPORT cs_setCatchMultiSignal(bool enable);

    void UQM_EXPORT cs_setUnwindExtraStack(bool enable);


#if __APPLE__
    bool cs_showRatingAlertAdapter();
    void cs_showAppStoreProductViewAdapter(const char* appid);
    #endif
}

#endif //ANDROID_UQMUNITYEXTRA_H
