#mirrorMotion = scene.getMotion("ChrUsaMleAdult@Idle01_ToWalkRt01")
#mirrorMotion.mirror("ChrUsaMleAdult@Idle01_ToWalkLf01", "ChrGarza.sk")

stateManager = scene.getStateManager()

# transition1 = stateManager.createTransition("ChrGarzaStartingLeft", "ChrGarzaLocomotion")
# transition1.addCorrespondancePoint("ChrUsaMleAdult@Idle01_ToWalkLf01", "ChrUsaMleAdult@Walk01", 0.54, 0.85, 0.00, 0.2)
# transition2 = stateManager.createTransition("ChrGarzaStartingRight", "ChrGarzaLocomotion")
# transition2.addCorrespondancePoint("ChrUsaMleAdult@Idle01_ToWalkRt01", "ChrUsaMleAdult@Walk01", 0.54, 0.9, 0.74, 0.93)

walkLeftTransitionIn = stateManager.createTransition("ChrMarineStartingLeft","ChrMarineLocomotion")
walkLeftTransitionIn.setEaseInInterval("ChrBrad_ChrMarine@Walk01", 1.32,1.56)
walkLeftTransitionIn.addEaseOutInterval("ChrBrad_ChrMarine@Idle01_ToWalkLf01",1.34,1.56)


walkRightTransitionIn = stateManager.createTransition("ChrMarineStartingRight","ChrMarineLocomotion")
walkRightTransitionIn.setEaseInInterval("ChrBrad_ChrMarine@Walk01", 0.75,1.08)
walkRightTransitionIn.addEaseOutInterval("ChrBrad_ChrMarine@Idle01_ToWalk01",1.33,1.56)


#jumpTransitionIn = stateManager.createTransition("ChrMarineLocomotion", "ChrMarineRunJumpState")
#jumpTransitionIn.setEaseInInterval("ChrBrad_ChrMarine@Run01_JumpHigh01", 0.00,0.2)
#jumpTransitionIn.addEaseOutInterval("ChrBrad_ChrMarine@Run01",0.47,0.67)

#jumpTransitionOut = stateManager.createTransition( "ChrMarineRunJumpState","ChrMarineLocomotion")
#jumpTransitionOut.setEaseInInterval("ChrBrad_ChrMarine@Run01",0.33,0.5)
#jumpTransitionOut.addEaseOutInterval("ChrBrad_ChrMarine@Run01_JumpHigh01",1.10,1.35);
