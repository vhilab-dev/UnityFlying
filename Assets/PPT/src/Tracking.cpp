// Melvin Low: mwlow@cs.stanford.edu
// Build in DEBUG Mode; as of right now the VRPN dll is buggy in release mode.

#include "stdafx.h"
#include "Tracking.h"

#define VRPN_INCLUDE_INTERSENSE
#include <VRPN/vrpn_Tracker_isense.h>
#include <VRPN/vrpn_Tracker.h>
#include <VRPN/vrpn_Analog.h>
#include <VRPN/vrpn_Button.h>

#include <string.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

extern "C" // Required for Unity plugins
{
    
    // InterSense Wrapper - assumes only ONE such device is connected.
    static vrpn_Connection *connection;
    static vrpn_Tracker_InterSense* intersense_device;
    static vrpn_Tracker_Remote *intersense_remote;
    static double intersense_quaternion[4];
    
    void VRPN_CALLBACK InterSenseCallback(void *userData, const vrpn_TRACKERCB t)
    {
        // Ninja quaternion data into our array.
        memcpy(intersense_quaternion, t.quat, 4 * sizeof(double));
    }
    
    void EXPORT_API FetchInterSenseQuaternion(double *out_quaternion)
    {
        // Ninja quaternion data to Unity.
        memcpy(out_quaternion, intersense_quaternion, 4 * sizeof(double));
    }
    
    void EXPORT_API UpdateInterSense()
    {
        // Update both the device and the connected remote.
        intersense_device->mainloop();
        intersense_remote->mainloop();
    }
    
    void EXPORT_API InitializeInterSense(int comm_port)
    {
        // Initialize the InterSense device connected to 'comm_port' serial port.
        // Opens a server connection to the InterSense hardware, then connects to it.
        // Connetion -> Device -> Remote
        connection = vrpn_create_server_connection(vrpn_DEFAULT_LISTEN_PORT_NO);
        intersense_device = new vrpn_Tracker_InterSense("InertiaCube", connection, comm_port);
        intersense_remote = new vrpn_Tracker_Remote("InertiaCube", connection);
        
        // What happens when the remote updates.
        intersense_remote->register_change_handler(0, InterSenseCallback);
    }
    
    void EXPORT_API TerminateInterSense()
    {
        // Release unmanaged memory and close the server connection.
        intersense_remote->unregister_change_handler(0, InterSenseCallback);
        intersense_remote->~vrpn_Tracker_Remote();
        intersense_device->~vrpn_Tracker_InterSense();
        connection->~vrpn_Connection();
    }
    
    
    // PPT Wrapper
    static vrpn_Tracker_Remote *ppt_remote;
    static double *ppt_positions;
    
    void VRPN_CALLBACK PPTCallback(void *userData, const vrpn_TRACKERCB t)
    {
        // Ninja the tracker data into our array.
        memcpy(ppt_positions + t.sensor * 3, t.pos, 3 * sizeof(double));
    }
    
    void EXPORT_API FetchPPTPosition(double *out_position, const int tracker_id)
    {
        // Ninja the tracker data to Unity.
        memcpy(out_position, ppt_positions + tracker_id * 3, 3 * sizeof(double));
    }
    
    void EXPORT_API UpdatePPT()
    {
        // Update the tracker.
        ppt_remote->mainloop();
    }
    
    void EXPORT_API InitializePPT(const char *ppt_address, const int max_trackers)
    {
        // Connect to the PPT server running at the specified address.
        // For example: "PPT0@171.64.33.43"
        ppt_remote = new vrpn_Tracker_Remote(ppt_address);
        ppt_remote->register_change_handler(0, PPTCallback);
        
        // Allocate memory to store trackers.
		ppt_positions = (double *)calloc(3 * (max_trackers + 1), sizeof(double));
    }
    
    void EXPORT_API TerminatePPT()
    {
        // Release unmanaged memory.
        ppt_remote->unregister_change_handler(0, PPTCallback);
        ppt_remote->~vrpn_Tracker_Remote();
        free(ppt_positions);
    }

	// Wand Wrapper
	static vrpn_Analog_Remote* wand_analog;
	static vrpn_Button_Remote* wand_button;
    
	static int button_states[6];
	static double analog_data[2];

	static void VRPN_CALLBACK WandAnalogCallback(void *userdata, const vrpn_ANALOGCB a)
	{
		memcpy(analog_data, a.channel, 2 * sizeof(double));
	}

	static void VRPN_CALLBACK WandButtonCallback(void *userdata, const vrpn_BUTTONCB b)
	{
		button_states[b.button] = b.state;
	}
    
	void EXPORT_API FetchWandButtonStates(int *out_buttonstates)
    {
        memcpy(out_buttonstates, button_states, 6 * sizeof(int));
    }

	void EXPORT_API FetchWandAnalogData(double *out_analogdata)
    {
        memcpy(out_analogdata, analog_data, 2 * sizeof(double));
    }
    
    void EXPORT_API UpdateWand()
    {
        wand_analog->mainloop();
		wand_button->mainloop();
    }
    
    void EXPORT_API InitializeWand(const char *wand_address)
    {
		wand_analog = new vrpn_Analog_Remote(wand_address);
		wand_button = new vrpn_Button_Remote(wand_address);

		wand_analog->register_change_handler(0, WandAnalogCallback);
		wand_button->register_change_handler(0, WandButtonCallback);
    }
    
    void EXPORT_API TerminateWand()
    {
        wand_analog->unregister_change_handler(0, WandAnalogCallback);
		wand_button->unregister_change_handler(0, WandButtonCallback);

		wand_analog->~vrpn_Analog_Remote();
		wand_button->~vrpn_Button_Remote();
    }
}