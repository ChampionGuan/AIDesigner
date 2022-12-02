//
//  UJSInterface.h
//  Unity-iPhone
//
//  Created by MacMini on 14-5-15.
//
//

#import <Foundation/Foundation.h>

@interface IPV6Interface : NSObject
 
+(NSString *)getIPv6 : (const char *)mHost :(const char *)mPort;
 
@end