//
//  UQMCrashManager.h
//  CrashSight
//
//  Created by joyfyzhang on 2020/9/4.
//  Copyright © 2020 joyfyzhang. All rights reserved.
//

#ifndef UQMCrashManager_h
#define UQMCrashManager_h

#include "UQMDefine.h"
#include "UQMCrash.h"

NS_UQM_BEGIN

class UQMCrashManager : public UQMSingleton<UQMCrashManager>
{
    friend class UQMSingleton<UQMCrashManager>;
    
public:
    static void ConfigCallbackTypeBeforeInit(int32_t callbackType);
    
    static void ConfigCrashHandleTimeout(int32_t timeout);

    void Init(const std::string& appId, bool unexpectedTerminatingDetectionEnable, bool debugMode, const std::string& serverUrl);
    
    static void LogInfo(int level, const std::string& tag, const std::string& log);
    
    static void SetUserValue(const std::string& key, const std::string& value);
    
    static void SetUserId(std::string userId);

    static void SetAppId(std::string appId);

    static void SetUserSceneTag(std::string userSceneTag);
    
    static void ReportException(int type, const std::string& exceptionName, const std::string& exceptionMsg, const std::string& exceptionStack, const UQMVector<UQMKVPair> &extInfo, const std::string& extInfoJsonStr, bool quit= false, int dumpNativeType= 0);

    static void ReportLogInfo(const char *msgType, const char *msg);

    static void SetIsAppForeground(bool isAppForeground);

    static void SetAppVersion(const std::string& appVersion);

    // 测试接口
    static void TestOomCrash();
    static void TestJavaCrash();
    static void TestOcCrash();
    static void TestNativeCrash();

    // agent
    
    void InitWithAppId (const std::string& appId);

    static void SetGameType(int gameType);
    
    static void ConfigDefaultBeforeInit(const std::string& appChannel, const std::string& version, const std::string& user, long delay);
    
    static void ConfigCrashServerUrlBeforeInit(const std::string& serverUrl);

    static void ConfigCrashReporterLogLevelBeforeInit(int logLevel);

    static void ConfigDebugModeBeforeInit(bool enable);

    static void SetDeviceId(const std::string& deviceId);

    static void SetDeviceModel(const std::string& deviceModel);

    static void SetLogPath(const std::string& logPath);

    static void SetScene (int sceneId);

    static void LogRecord (int level, const std::string& message);

    static int GetPlatformCode();

    static void CloseCrashReport();

    static void StartCrashReport();

    static void SetCatchMultiSignal(bool enable);

    static void SetUnwindExtraStack(bool enable);

private:
    bool mIsInit;
    UQMVector<UQMString> mChannel;
    
    UQMCrashManager() : mIsInit(false) {};
    
};

NS_UQM_END

#endif /* UQMCrashManager_h */
