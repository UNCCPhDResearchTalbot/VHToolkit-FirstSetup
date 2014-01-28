
print "SB - init-reach-grasp.py"


# handle grasp event
# grasp event currently uses old-style command interface
# by storing the command as the parameter of the event
class GraspHandler(EventHandler):
    def executeAction(this, event):
        params = event.getParameters()
        scene.command(params)

# now create the event handler for the 'reach' event
graspHandler = GraspHandler()
em = scene.getEventManager()
em.addEventHandler("reach", graspHandler)
