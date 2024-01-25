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
	//std::cout << "Hello World!\n";

	char sendBuffer[] = { 0x41,0x42,0x43,0x44,0x45,0x46,0x47,0x48,0x49,0x50, 0x41,0x42,0x43,0x44,0x45,0x46,0x47,0x48,0x49,0x50 };
	int sendCount;
	int i, j;
	CUeiSession mySession;
	//CUeiSerialReader** readers;
	CUeiSerialWriter* writer;

	string resString("pdna://192.168.100.3/Dev3/com0");

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

	writer = new CUeiSerialWriter(mySession.GetDataStream(), port1);

	mySession.Start();

	sendCount = 0;
	for (int m = 0; m < 50000; m++)
	{
		// do write for each channel
		//for (i = 0; i < mySession.GetNumberOfChannels(); i++)
		{
			//for (j = 0; j < SEND_SIZE; j++) {
			//    sendBuffer[j] = 'a' + sendCount;
			//    sendCount = (sendCount + 1) % 26;
			//}
			//sendBuffer[SEND_SIZE] = 0;
			cout << m << "  sending " << sizeof(sendBuffer) << " bytes to " << " ch:" << port1 << "\n";
				//" % d bytes to  writer[% d]: % s\n", i, sendBuffer);
			writer->Write(sizeof(sendBuffer), sendBuffer, NULL);
		}
		//printf("\n");

		Sleep(1);
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
