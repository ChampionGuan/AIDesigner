#pragma once

#ifndef GCLOUD_VERSION_CRASHSIGHT
#define GCLOUD_VERSION_CRASHSIGHT "GCLOUD_VERSION_CRASHSIGHT_4.2.11.751" //用于编译时传入
#endif
//#define PLATFORM_IOS 0

#include <string.h>
#include <stdlib.h>

#include "UQMMacros.h"

namespace GCloud {
namespace CrashSight {

enum LogSeverity
{
    Log,
    LogDebug,
    LogInfo,
    LogWarning,
    LogAssert,
    LogError,
    LogException
};

class UQM_EXPORT CrashSightAgent
{
public:

    /// Set game type for Android
    /// @param gameType COCOS=1, UNITY=2, UNREAL=3
    static void SetGameType(int gameType);

    /// configs callback type.
    /// @param callbackType 目前是5种类型，用5位表示。第一位表示crash，第二位表示anr，第三位表示u3d c# error，第四位表示js，第五位表示lua
    static void ConfigCallbackType (int32_t callbackType);

    /// Configs the default.
    /// @param channel channel
    /// @param version version
    /// @param user user
    /// @param delay delay
    static void ConfigDefault (const char *channel, const char *version, const char * user, long delay);

    /// Configs the crashServerUrl.
    /// @param crashServerUrl crashServerUrl
    static void ConfigCrashServerUrl(const char *crashServerUrl);
    
    /// Configs the type of the crash reporter and customized log level to upload
    /// @param logLevel Off=0,Error=1,Warn=2,Info=3,Debug=4
    static void ConfigCrashReporter(int logLevel);

    /// configs the debug mode.
    /// @param enable If set to true< debug mode.
    static void ConfigDebugMode (bool enable);

    /// Set deviceId.
    /// @param deviceId 设备唯一标识
    static void SetDeviceId(const char *deviceId);

    /// Set deviceModel.
    /// @param deviceModel 手机型号
    static void SetDeviceModel(const char *deviceModel);

    /// 设置日志绝对路径.
    /// @param logPath 日志路径
    static void SetLogPath(const char *logPath);

    /// Init sdk with the specified appId.
    /// @param appId App identifier.
    static void InitWithAppId (const char *appId);

    /// Report Exception
    /// @param type type
    /// @param name name
    /// @param reason reason
    /// @param stackTrace stackTrace， json数组序列化字符串
    /// @param extras extras, json对象序列化字符串
    /// @param quit quit
    /// @param dumpNativeType 0：关闭，1：调用系统接口dump，3：minidump
    static void ReportException(int type, const char * name, const char * reason, const char * stackTrace, const char * extras, bool quit, int dumpNativeType= 0);

    /// Report Exception
    /// @param type
    /// @param exceptionName
    /// @param exceptionMsg
    /// @param exceptionStack
    /// @param paramsJson map序列化后的JSON字符串
    /// @param dumpNativeType 0：关闭，1：调用系统接口dump，3：minidump
    static void ReportExceptionJson(int type, const char *exceptionName, const char *exceptionMsg, const char *exceptionStack, const char *paramsJson, int dumpNativeType =0);

    /// Report log statistics
    /// @msgType 消息类型
    /// @msg 消息详情
    static void ReportLogInfo(const char *msgType, const char *msg);

    /// Sets the user identifier.
    /// @param userId User identifier.
    static void SetUserId (const char *userId);

    /// Sets the scene.
    /// @param sceneId Scene identifier.
    static void SetScene (int sceneId);

    /// Adds the scene data.
    /// @param key Key
    /// @param value Value
    static void AddSceneData (const char *key, const char *value);

    /// Prints the log.
    /// @param level level
    /// @param format format
    static void PrintLog (LogSeverity level, const char * format, ...);

	static int GetPlatformCode();

	// unity android
	static void CloseCrashReport();

    // unity android
	static void StartCrashReport();

    // unity android
	static void RestartCrashReport();

    /// Set app version.
    /// @param appVersion app version
	static void SetAppVersion(const char *appVersion);


    /// Catch multiple signal from different thread, and upload information of first signal.
    /// @param SetCatchMultiSignal enable
    static void SetCatchMultiSignal(bool enable);

    /// Unwind at most 256 stack frame, and report last frame even if stack string is full.
    /// @param SetUnwindExtraStack enable
    static void SetUnwindExtraStack(bool enable);


    static void TestOomCrash();

    static void TestJavaCrash();

    static void TestOcCrash();

    static void TestNativeCrash();

private:
    static bool mIsInitialized;

    static const char* GetCsVersion();
    
};


}
}





