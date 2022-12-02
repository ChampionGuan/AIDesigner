//
//  UQMCrashDelegate.h
//  UQMCore
//
//  Created by joyfyzhang on 2021/1/8.
//  Copyright © 2021 joyfyzhang. All rights reserved.
//

#ifndef UQMCrashDelegate_h
#define UQMCrashDelegate_h

#import <Foundation/Foundation.h>

@protocol UQMCrashDelegate <NSObject>

@required

- (void)configCallbackTypeBeforeInit: (int32_t)callbackType;

- (void)configCrashHandleTimeout: (int32_t)timeout;

- (void)initCrashReport:(NSString *)appId unexpectedTerminatingDetectionEnable:(bool)unexpectedTerminatingDetectionEnable debugMode:(bool)debugMode serverUrl:(NSString *)serverUrl;

@optional

- (void)reportLog:(int)level tag:(NSString *)tag log:(NSString *)log;

- (void)setUserData:(NSString *)key value:(NSString *)value;

- (void)setUserId: (NSString *)userId;

- (void)setAppId: (NSString *)appId;

- (void)setUserSceneTag: (NSString *)userSceneTag;

- (void)reportException:(int)type exceptionName:(NSString *)exceptionName
        exceptionMsg:(NSString *)exceptionMsg
        exceptionStack:(NSString *)exceptionStack
        extInfo:(NSDictionary *)extInfo
        extInfoJsonStr:(NSString *)extInfoJsonStr
        quit:(bool)quit
        dumpNativeType:(int)dumpNativeType;

-(void)reportLogInfo:(NSString*)msgType msg:(NSString*)msg;

- (void)setIsAppForeground: (bool)isAppForeground;

- (void)setAppVersion: (NSString *)appVersion;

// agent
- (void)initWithAppId:(NSString *) appId;

- (void)configCrashServerUrlBeforeInit: (NSString *)serverUrl;

- (void)configDefaultBeforeInit: (NSString *)channel version:(NSString *)version user:(NSString *)user delay:(long)delay;

- (void)configCrashReporterLogLevelBeforeInit: (int)logLevel;

- (void)configDebugModeBeforeInit: (bool) enable;

- (void)setScene: (int)sceneId;

- (void)logRecord: (int)level message: (NSString *)message;

- (int)getPlatformCode;

- (void)setLogPath: (NSString *)logPath;

-(void)closeCrashReport;

-(void)reregisterCrashHandler;

@end

#endif /* UQMCrashDelegate_h */
