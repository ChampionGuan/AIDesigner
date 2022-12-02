
#import "IOSAlbumCameraController.h"

static bool _IsInAlbuming = false;
static bool _IsPickedAlbum = false;
@implementation IOSAlbumCameraController

- (void)showActionSheet
{
    NSLog(@" --- showActionSheet !!");
    
    UIAlertController *alertController = [UIAlertController alertControllerWithTitle:nil message:nil preferredStyle:UIAlertControllerStyleActionSheet];
    
    UIAlertAction *albumAction = [UIAlertAction actionWithTitle:@"相册" style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action){
        NSLog(@"click album action!");
        [self showPicker:UIImagePickerControllerSourceTypePhotoLibrary allowsEditing:YES];
    }];
    
    UIAlertAction *cameraAction = [UIAlertAction actionWithTitle:@"相机" style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action){
        NSLog(@"click camera action!");
        [self showPicker:UIImagePickerControllerSourceTypeCamera allowsEditing:YES];
    }];
    
    UIAlertAction *album_cameraAction = [UIAlertAction actionWithTitle:@"相册&相机" style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action){
        NSLog(@"click album&camera action!");
        //[self showPicker:UIImagePickerControllerSourceTypeCamera];
        [self showPicker:UIImagePickerControllerSourceTypeSavedPhotosAlbum allowsEditing:YES];
    }];
    
    UIAlertAction *cancelAction = [UIAlertAction actionWithTitle:@"取消" style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action){
        NSLog(@"click cancel action!");
    }];
    
    
    [alertController addAction:albumAction];
    [alertController addAction:cameraAction];
    [alertController addAction:album_cameraAction];
    [alertController addAction:cancelAction];
    
    UIViewController *vc = UnityGetGLViewController();
    [vc presentViewController:alertController animated:YES completion:^{
        NSLog(@"showActionSheet -- completion");
    }];
}

- (void)showPicker:(UIImagePickerControllerSourceType)type allowsEditing:(BOOL)flag
{
    NSLog(@" --- showPicker!!");
    UIImagePickerController *picker = [[UIImagePickerController alloc] init];
    picker.delegate = self;
    picker.sourceType = type;
    picker.allowsEditing = flag;
    
    [self presentViewController:picker animated:YES completion:nil];
}

// 打开相册后选择照片时的响应方法
- (void)imagePickerController:(UIImagePickerController*)picker didFinishPickingMediaWithInfo:(NSDictionary*)info
{
    NSLog(@" --- imagePickerController didFinishPickingMediaWithInfo!!");
	if(_IsInAlbuming == false)
	{
		return;
	}
	
	_IsPickedAlbum = true;
	_IsInAlbuming = false;
	
    UIImage *image = info[UIImagePickerControllerOriginalImage];
    image = [self fixOrientation:image];
     
    // 关闭相册
    [picker dismissViewControllerAnimated:YES completion:^{
        [self.view removeFromSuperview];
    }];
	
    // 得到了image，然后用你的函数传回u3d
	NSString* filename = @"ios_temp.png";
	[self TrySaveFile:image path:filename];
	UnitySendMessage( "AlbumListener", "IOSPhotoCallBack2", filename.UTF8String);
	
    //NSString *_encodeImageStr = [imgData base64EncodedStringWithOptions:kNilOptions];
    //UnitySendMessage( "AlbumListener", "IOSPhotoCallBack", _encodeImageStr.UTF8String);
}


-(NSString*)GetSavePath:(NSString *)filename{
    NSArray *pathArray = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *docPath = [pathArray objectAtIndex:0];
    return [docPath stringByAppendingPathComponent:filename];
}

-(void)TrySaveFile:(UIImage *)image path:(NSString *)path{
    NSData *imgData;
	imgData = UIImagePNGRepresentation(image);
	NSString *imagePath = [self GetSavePath:path];
    [imgData writeToFile:imagePath atomically:YES];
}

// 打开相册后点击“取消”的响应
- (void)imagePickerControllerDidCancel:(UIImagePickerController*)picker
{
    NSLog(@" --- imagePickerControllerDidCancel !!");
    
    //UnitySendMessage( "AlbumListener", "PhotoReturnCallBack", [NSString stringWithFormat:@"Cancel"].UTF8String);
    
    [self dismissViewControllerAnimated:YES completion:^{
        [self.view removeFromSuperview];
    }];
}

+ (void)saveImageToPhotosAlbum:(NSString*)readAdr
{
    readAdr = [NSString stringWithFormat:@"%@.png", readAdr];
    UIImage* image = [UIImage imageWithContentsOfFile:readAdr];
    NSLog(@"Tackor ------>> %@  /n %@ w:%f, h:%f", readAdr, image, image.size.width, image.size.height);
    UIImageWriteToSavedPhotosAlbum(image,
                                   self,
                                   @selector(image:didFinishSavingWithError:contextInfo:),
                                   NULL);
}

+ (void)image:(UIImage *)image didFinishSavingWithError:(NSError*)error contextInfo:(void *)contextInfo
{
    NSString* result;
    if(error)
    {
        result = @"图片保存到相册失败!";
    }
    else
    {
        result = @"图片保存到相册成功!";
    }
    //UnitySendMessage( "AlbumListener", "SaveImageToPhotosAlbumCallBack", result.UTF8String);
}

///大于2m的图片居然会旋转, 用这个方法规避旋转问题
- (UIImage *)fixOrientation:(UIImage *)aImage {
    
    // No-op if the orientation is already correct
    if (aImage.imageOrientation == UIImageOrientationUp)
        return aImage;
    
    // We need to calculate the proper transformation to make the image upright.
    // We do it in 2 steps: Rotate if Left/Right/Down, and then flip if Mirrored.
    CGAffineTransform transform = CGAffineTransformIdentity;
    
    switch (aImage.imageOrientation) {
        case UIImageOrientationDown:
        case UIImageOrientationDownMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.width, aImage.size.height);
            transform = CGAffineTransformRotate(transform, M_PI);
            break;
            
        case UIImageOrientationLeft:
        case UIImageOrientationLeftMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.width, 0);
            transform = CGAffineTransformRotate(transform, M_PI_2);
            break;
            
        case UIImageOrientationRight:
        case UIImageOrientationRightMirrored:
            transform = CGAffineTransformTranslate(transform, 0, aImage.size.height);
            transform = CGAffineTransformRotate(transform, -M_PI_2);
            break;
        default:
            break;
    }
    
    switch (aImage.imageOrientation) {
        case UIImageOrientationUpMirrored:
        case UIImageOrientationDownMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.width, 0);
            transform = CGAffineTransformScale(transform, -1, 1);
            break;
            
        case UIImageOrientationLeftMirrored:
        case UIImageOrientationRightMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.height, 0);
            transform = CGAffineTransformScale(transform, -1, 1);
            break;
        default:
            break;
    }
    
    // Now we draw the underlying CGImage into a new context, applying the transform
    // calculated above.
    CGContextRef ctx = CGBitmapContextCreate(NULL, aImage.size.width, aImage.size.height,
                                             CGImageGetBitsPerComponent(aImage.CGImage), 0,
                                             CGImageGetColorSpace(aImage.CGImage),
                                             CGImageGetBitmapInfo(aImage.CGImage));
    CGContextConcatCTM(ctx, transform);
    switch (aImage.imageOrientation) {
        case UIImageOrientationLeft:
        case UIImageOrientationLeftMirrored:
        case UIImageOrientationRight:
        case UIImageOrientationRightMirrored:
            // Grr...
            CGContextDrawImage(ctx, CGRectMake(0,0,aImage.size.height,aImage.size.width), aImage.CGImage);
            break;
            
        default:
            CGContextDrawImage(ctx, CGRectMake(0,0,aImage.size.width,aImage.size.height), aImage.CGImage);
            break;
    }
    
    // And now we just create a new UIImage from the drawing context
    CGImageRef cgimg = CGBitmapContextCreateImage(ctx);
    UIImage *img = [UIImage imageWithCGImage:cgimg];
    CGContextRelease(ctx);
    CGImageRelease(cgimg);
    return img;
}

- (void)viewWillDisappear:(BOOL)animated {
    [super viewWillDisappear:animated];
    NSLog(@"viewWillDisappear === DISAPPEAR");
	if(_IsPickedAlbum  == false)
	{
	    UnitySendMessage( "AlbumListener", "PhotoReturnCallBack", [NSString stringWithFormat:@"Cancel"].UTF8String);
	}
	_IsPickedAlbum = false;
}

@end


//------------- called by unity -----begin-----------------
#if defined (__cplusplus)
extern "C" {
#endif
    
    // 弹出一个菜单项：相册、相机
    void _showActionSheet()
    {
        NSLog(@" -unity call-- _showActionSheet !!");
        IOSAlbumCameraController * app = [[IOSAlbumCameraController alloc] init];
        UIViewController *vc = UnityGetGLViewController();
        [vc.view addSubview: app.view];
        
        [app showActionSheet];
    }
    
    // 打开相册
    void _iosOpenPhotoLibrary()
    {
        if([UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypePhotoLibrary])
        {
            IOSAlbumCameraController * app = [[IOSAlbumCameraController alloc] init];
            UIViewController *vc = UnityGetGLViewController();
            [vc.view addSubview: app.view];
            
			_IsInAlbuming = true;
            [app showPicker:UIImagePickerControllerSourceTypePhotoLibrary allowsEditing:NO];
        }
    }
    
    // 打开相册
    void _iosOpenPhotoAlbums()
    {
        if([UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypeSavedPhotosAlbum])
        {
            IOSAlbumCameraController * app = [[IOSAlbumCameraController alloc] init];
            UIViewController *vc = UnityGetGLViewController();
            [vc.view addSubview: app.view];
            
			_IsInAlbuming = true;
            [app showPicker:UIImagePickerControllerSourceTypeSavedPhotosAlbum allowsEditing:NO];
        }
        else
        {
            _iosOpenPhotoLibrary();
        }
    }
    
    // 打开相机
    void _iosOpenCamera()
    {
        if([UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypeCamera])
        {
            IOSAlbumCameraController * app = [[IOSAlbumCameraController alloc] init];
            UIViewController *vc = UnityGetGLViewController();
            [vc.view addSubview: app.view];
            
            [app showPicker:UIImagePickerControllerSourceTypeCamera allowsEditing:NO];
        }
    }
    
    
    // 打开相册--可编辑
    void _iosOpenPhotoLibrary_allowsEditing()
    {
        if([UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypePhotoLibrary])
        {
            IOSAlbumCameraController * app = [[IOSAlbumCameraController alloc] init];
            UIViewController *vc = UnityGetGLViewController();
            [vc.view addSubview: app.view];
            
			_IsInAlbuming = true;
            [app showPicker:UIImagePickerControllerSourceTypePhotoLibrary allowsEditing:YES];
        }
        
    }
    
    // 打开相册--可编辑
    void _iosOpenPhotoAlbums_allowsEditing()
    {
        if([UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypeSavedPhotosAlbum])
        {
            IOSAlbumCameraController * app = [[IOSAlbumCameraController alloc] init];
            UIViewController *vc = UnityGetGLViewController();
            [vc.view addSubview: app.view];
            
			_IsInAlbuming = true;
            [app showPicker:UIImagePickerControllerSourceTypeSavedPhotosAlbum allowsEditing:YES];
        }
        else
        {
            _iosOpenPhotoLibrary();
        }
        
    }
    
    // 打开相机--可编辑
    void _iosOpenCamera_allowsEditing()
    {
        if([UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypeCamera])
        {
            IOSAlbumCameraController * app = [[IOSAlbumCameraController alloc] init];
            UIViewController *vc = UnityGetGLViewController();
            [vc.view addSubview: app.view];
            
            [app showPicker:UIImagePickerControllerSourceTypeCamera allowsEditing:YES];
        }
    }
    
    // 保存照片到相册
    void _iosSaveImageToPhotosAlbum(char* readAddr)
    {
        NSString* temp = [NSString stringWithUTF8String:readAddr];
        [IOSAlbumCameraController saveImageToPhotosAlbum:temp];
    }
    
#if defined (__cplusplus)
}
#endif
