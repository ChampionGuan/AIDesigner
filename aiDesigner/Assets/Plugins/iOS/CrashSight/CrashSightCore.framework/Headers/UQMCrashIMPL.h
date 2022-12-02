//
//  UQMCrashIMPL.h
//  CrashSight
//
//  Created by joyfyzhang on 2020/9/4.
//  Copyright © 2020 joyfyzhang. All rights reserved.
//

#ifndef UQMCrashIMPL_h
#define UQMCrashIMPL_h

#include "UQMDefine.h"
#include "UQMSingleton.h"
#include "UQMCompatLayer.h"
#include "CrashSightAgent.h"

using namespace GCloud::CrashSight;

NS_UQM_BEGIN

class UQMCrashIMPL : public UQMSingleton<UQMCrashIMPL>
{
    friend class UQMSingleton<UQMCrashIMPL>;
    
public:

    static void ConfigCallbackTypeBeforeInit(const std::string& channel, int32_t callbackType);
    
    static void ConfigCrashHandleTimeout(const std::string& channel, int32_t timeout);

    static bool Init(const std::string& channel, const std::string& appId, bool unexpectedTerminatingDetectionEnable, bool debugMode, const std::string& serverUrl);
    
    static void LogInfo(const std::string& channel, int level, const std::string& tag, const std::string& log);
    
    static void SetUserValue(const std::string& channel, const std::string& key, const std::string& value);
    
    static void SetUserId(const std::string& channel, const std::string& userId);

    static void SetAppId(const std::string& channel, const std::string& appId);

    static void SetUserSceneTag(const std::string& channel, const std::string& userSceneTag);
    
    static void ReportException(const std::string& channel, int type, const std::string& exceptionName, const std::string& exceptionMsg, const std::string& exceptionStack, const UQMVector<UQMKVPair> &extInfo, const std::string& extInfoJsonStr, bool quit= false, int dumpNativeType= 0);

    static void ReportLogInfo(const std::string& msgType, const std::string& msg);

#ifdef ANDROID
    static jobject convert(const std::map<std::string, std::string> &data);
#endif

    static void SetIsAppForeground(const std::string& channel, bool isAppForeground);

    static void SetAppVersion(const std::string& channel, const std::string& appVersion);

    //测试接口
    static void TestOomCrash(const std::string& channel);
    static void TestJavaCrash(const std::string& channel);
    static void TestOcCrash(const std::string& channel);
    static void TestNativeCrash(std::string channel) {
        UQM_LOG_DEBUG("TestNativeCrash channel = %s", channel.c_str());
        abort();
    }

    //agent
    static bool InitWithAppId (const std::string& channel, const std::string& appId);

    static void SetGameType(const std::string& channel, int gameType);

    static void ConfigDefaultBeforeInit(const std::string& channel, const std::string& appChannel, const std::string& version, const std::string& user, long delay);
    
    static void ConfigCrashServerUrlBeforeInit(const std::string& channel, const std::string& serverUrl);

    static void ConfigCrashReporterLogLevelBeforeInit(const std::string& channel, int logLevel);

    static void ConfigDebugModeBeforeInit(const std::string& channel, bool enable);

    static void SetDeviceId(const std::string& channel, const std::string& deviceId);

    static void SetDeviceModel(const std::string& channel, const std::string& deviceModel);

    static void SetLogPath(const std::string& channel, const std::string& logPath);

    static void SetScene (const std::string& channel, int sceneId);

    static void LogRecord (const std::string& channel, int level, const std::string& message);

    static void CloseCrashReport(const std::string& channel);

    static void StartCrashReport(const std::string& channel);

    static int GetPlatformCode(const std::string& channel);

    static void SetCatchMultiSignal(const std::string& channel, bool enable);

    static void SetUnwindExtraStack(const std::string& channel, bool enable);


private:
    UQMCrashIMPL() {}
    static void CallFunction(const std::string& channel, const std::string& functionName, bool enable);
};

NS_UQM_END

#endif /* UQMCrashIMPL_h */
