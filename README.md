btComPrint
==========

Windows Mobile print via COM to BT printer using RegisterDevice

TODO:
*  remove obsolete classes etc. For RegisterDevcice we only need PPPortEmuParms, SDP Service GUID and BTAddress. No BluetoothEndpoint etc.
*  Add BTH_SetPin or similar to enable PIN enabled connections. Although currently the devices do not ask for a PIN
