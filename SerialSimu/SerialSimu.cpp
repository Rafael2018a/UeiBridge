// SerialSimu.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <Windows.h>
#include "ueidaq.h"
using namespace UeiDaq;
using namespace std;
#define SEND_SIZE  9         // amount of data to send each iteration
#define RECV_SIZE  4         // max amount of data to receive on each read
#define WATERMARK  5         // amount of data to trigger event
#define TIMEOUT_uS 1000000   // time without data to trigger event

int main()
{
	//printf("date: '%s'\n", __DATE__);
	//printf("time: '%s'\n", __TIME__);
	//printf("timestamp: '%s'\n", __TIMESTAMP__);

	cout << "Compilation time: " << __TIMESTAMP__ << "\n";

	//std::cout << "Hello World!\n";

	char sendBuffer[100];// = { 0x41,0x42,0x43,0x44,0x45,0x46,0x47,0x48,0x49,0x50, 0x41,0x42,0x43,0x44,0x45,0x46,0x47,0x48,0x49,0x50 };
	//int sendCount;
	//int i, j;
	CUeiSession mySession;
	//CUeiSerialReader** readers;
	//CUeiSerialWriter* writer;
	vector<CUeiSerialWriter*> writers;

	

	string resString("pdna://192.168.100.3/Dev3/com0");

	CUeiDevice * device = CUeiDeviceEnumerator::GetDeviceFromResource(resString);
	device->Reset();

	CUeiSerialPort* port = mySession.CreateSerialPort(resString,
		UeiSerialModeRS485FullDuplex,
		UeiSerialBitsPerSecond57600,
		UeiSerialDataBits8,
		UeiSerialParityNone,
		UeiSerialStopBits1,
		"");
	mySession.ConfigureTimingForAsynchronousIO(WATERMARK, 0, TIMEOUT_uS, 0);
	mySession.GetTiming()->SetTimeout(10);

	cout << "Writing to " << resString << "at 57600bps, RS485FullDuplex" << "\n";


	//// Create an asynchronous listener and a writer for each port configured
	//// in the resource string
	//writer = new CUeiSerialWriter [mySession.GetNumberOfChannels()];
	//readers = new CUeiSerialReader * [mySession.GetNumberOfChannels()];
	int com1 = 0;
	Int32 port1 = mySession.GetChannel(com1)->GetIndex();

	writers.push_back( new CUeiSerialWriter(mySession.GetDataStream(), port1));
	//writer = new CUeiSerialWriter(mySession.GetDataStream(), port1);

	mySession.Start();

	for (int m = 0; m < 100; m++)
	{
		int seed = m + 40;
		// do write for each channel
		for (int i = 0; i < sizeof(sendBuffer); i++)
		{
			sendBuffer[i] = seed + i;
		}
        cout << std::dec << m << "  sending " << sizeof(sendBuffer) << " bytes through " << " ch:" << port1 << " first: 0x" << std::hex << seed << "\n";
		Int32 numberOfSent = 0;
		writers[0]->Write(sizeof(sendBuffer), sendBuffer, &numberOfSent);
		if (numberOfSent != sizeof(sendBuffer))
		{
			cout << "wrong length" << "\n";
		}
		Sleep(10);
	}

}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
