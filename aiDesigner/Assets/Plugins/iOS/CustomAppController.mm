#import "UnityAppController.h"
#import <PaperSDK/PSApi.h>

@interface CustomAppController:UnityAppController
@end
 
IMPL_APP_CONTROLLER_SUBCLASS (CustomAppController)
 
@implementation CustomAppController
 
- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    /* unity层 逻辑代码 */
    //PaperSDK初始化
    [PSApi application:application didFinishLaunchingWithOptions:launchOptions];
    
    //PaperSDK回调通知
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(psNotification:) name:@"PS_UnitySendMessage" object:nil];

    /* unity层 逻辑代码 */
    return [super application:application didFinishLaunchingWithOptions:launchOptions];
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
    /* unity层 逻辑代码 */
    [super applicationDidBecomeActive:application];
    //PaperSDK应用程序复原
    [PSApi applicationDidBecomeActive:application];
}

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken
{
    /* unity层 逻辑代码 */
    [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
    //PaperSDK更新推送deviceToken
    [PSApi application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];

}

- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options {

    //PaperSDK处理openUrl
    return [PSApi application:app openURL:url options:options];
}
 
//返回PaperSDK通知给unity
-(void)psNotification:(NSNotification *)noti{
    
    NSDictionary  *dic = [noti userInfo];
    NSString *gameObjectStr = [dic objectForKey:@"gameObject"];
    NSString *funcName = [dic objectForKey:@"functionName"];
    NSString *jsonStr = [dic objectForKey:@"jsonStrChar"];
    
    const char * gameObject = [gameObjectStr UTF8String];
    const char * functionName = [funcName UTF8String];
    const char * jsonStrChar = [jsonStr UTF8String];
    UnitySendMessage(gameObject,functionName, jsonStrChar);
}

@end