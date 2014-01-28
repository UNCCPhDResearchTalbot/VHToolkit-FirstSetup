
import getpass
import os
import shutil
import subprocess
import sys


'''
unity - ios postprocess
/* Begin PBXBuildFile section */
		C41261A8161BEC3A00D56AD8 /* Accelerate.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = C41261A7161BEC3A00D56AD8 /* Accelerate.framework */; };

/* Begin PBXFileReference section */
		C41261A7161BEC3A00D56AD8 /* Accelerate.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = Accelerate.framework; path = System/Library/Frameworks/Accelerate.framework; sourceTree = SDKROOT; };

/* Begin PBXFrameworksBuildPhase section */
		1D60588F0D05DD3D006BFB54 /* Frameworks */ = {
			isa = PBXFrameworksBuildPhase;
			buildActionMask = 2147483647;
			files = (
				C41261A8161BEC3A00D56AD8 /* Accelerate.framework in Frameworks */,

/* Begin PBXGroup section */
		19C28FACFE9D520D11CA2CBB /* Products */ = {
			isa = PBXGroup;
			children = (
				1D6058910D05DD3D006BFB54 /* vhassets.app */,
				567B6273114A9F340000AA1F /* vhassets.app */,
			);
			name = Products;
			sourceTree = "<group>";
		};
		29B97314FDCFA39411CA2CEA /* CustomTemplate */ = {
			isa = PBXGroup;
			children = (
				C41261A7161BEC3A00D56AD8 /* Accelerate.framework */,

/* Begin XCBuildConfiguration section */
		1D6058940D05DD3E006BFB54 /* Debug */ = {
			isa = XCBuildConfiguration;
			buildSettings = {
				ONLY_ACTIVE_ARCH = YES;

		1D6058950D05DD3E006BFB54 /* Release */ = {
			isa = XCBuildConfiguration;
			buildSettings = {
				CODE_SIGN_IDENTITY = "iPhone Distribution";
				"CODE_SIGN_IDENTITY[sdk=iphoneos*]" = "iPhone Distribution";
				ONLY_ACTIVE_ARCH = YES;   ************ need to find/replace
				PROVISIONING_PROFILE = "";
				"PROVISIONING_PROFILE[sdk=iphoneos*]" = "";
'''




installPath = sys.argv[1]
f = open(installPath + "/Unity-iPhone.xcodeproj/project.pbxproj", "r")
lines = f.readlines()
f.close()


mode = 0

# make sure Accelerate.framework isn't already in list
for i in range(len(lines)):
    if "/* Accelerate.framework" in lines[i]:
        mode = 14
        break


while mode < 14:    # sometimes we break out, needing to start at the beginning
    for i in range(len(lines)):
        if mode == 0:
            if "/* Begin PBXBuildFile section */" in lines[i]:
                newLine = "		C41261A8161BEC3A00D56AD8 /* Accelerate.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = C41261A7161BEC3A00D56AD8 /* Accelerate.framework */; };"
                newLine += "\n"
                lines.insert(i+1, newLine)
                mode += 1



        elif mode == 1:
            if "/* Begin PBXFileReference section */" in lines[i]:
                newLine = "		C41261A7161BEC3A00D56AD8 /* Accelerate.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = Accelerate.framework; path = System/Library/Frameworks/Accelerate.framework; sourceTree = SDKROOT; };"
                newLine += "\n"
                lines.insert(i+1, newLine)
                mode += 1


        elif mode == 2:
            if "/* Begin PBXFrameworksBuildPhase section */" in lines[i]:
                mode += 1

        elif mode == 3:
            if "files = (" in lines[i]:
                newLine = "				C41261A8161BEC3A00D56AD8 /* Accelerate.framework in Frameworks */,"
                newLine += "\n"
                lines.insert(i+1, newLine)
                mode += 1


        elif mode == 4:
            if "/* Begin PBXGroup section */" in lines[i]:
                mode += 1

        elif mode == 5:
            if "/* CustomTemplate */" in lines[i]:
                mode += 1

        elif mode == 6:
            if "children = (" in lines[i]:
                newLine = "				C41261A7161BEC3A00D56AD8 /* Accelerate.framework */,"
                newLine += "\n"
                lines.insert(i+1, newLine)
                mode += 1


        elif mode == 7:
            if "/* Begin XCBuildConfiguration section */" in lines[i]:
                mode += 1

        elif mode == 8:
            if "/* Debug */" in lines[i]:
                mode += 1

        elif mode == 9:
            if "buildSettings = {" in lines[i]:
                #newLine = "				ONLY_ACTIVE_ARCH = YES;"
                #newLine += "\n"
                #lines.insert(i+1, newLine)

                # start over looking for release
                mode += 1
                break


        elif mode == 10:
            if "/* Begin XCBuildConfiguration section */" in lines[i]:
                mode += 1

        elif mode == 11:
            if "/* Release */" in lines[i]:
                mode += 1

        elif mode == 12:
            if "buildSettings = {" in lines[i]:
                newLine = '				CODE_SIGN_IDENTITY = "iPhone Distribution";'
                newLine += "\n"
                lines.insert(i+1, newLine)
                newLine = '				"CODE_SIGN_IDENTITY[sdk=iphoneos*]" = "iPhone Distribution";'
                newLine += "\n"
                lines.insert(i+1, newLine)
                newLine = '				PROVISIONING_PROFILE = "";'
                newLine += "\n"
                lines.insert(i+1, newLine)
                newLine = '				"PROVISIONING_PROFILE[sdk=iphoneos*]" = "";'
                newLine += "\n"
                lines.insert(i+1, newLine)
                mode += 1

        elif mode == 13:
            if "ONLY_ACTIVE_ARCH = NO;" in lines[i]:
                #lines[i] = "				ONLY_ACTIVE_ARCH = YES;"
                #lines[i] += "\n"
                mode += 1



f = open(installPath + "/Unity-iPhone.xcodeproj/project.pbxproj", "w")
f.writelines(lines)
f.close()


print os.getcwd()

username = getpass.getuser()
print username

os.makedirs(installPath + "/Unity-iPhone.xcodeproj/xcuserdata/{0}.xcuserdatad/xcschemes".format(username))
print 'unzip -o Assets/Editor/xcuserdata.zip -d {0}'.format(installPath + "/Unity-iPhone.xcodeproj/xcuserdata/{0}.xcuserdatad/xcschemes".format(username))
subprocess.call('unzip -o Assets/Editor/xcuserdata.zip -d {0}'.format(installPath + "/Unity-iPhone.xcodeproj/xcuserdata/{0}.xcuserdatad/xcschemes".format(username)).split(" "))
