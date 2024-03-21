// SerialSimu.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <sstream>  
#include <Windows.h>
#include "ueidaq.h"
using namespace UeiDaq;
using namespace std;
#define SEND_SIZE  9         // amount of data to send each iteration
//#define RECV_SIZE  4         // max amount of data to receive on each read
#define WATERMARK  5         // amount of data to trigger event
#define TIMEOUT_uS 1000000  // time without data to trigger event

char * BuildBuffer(int32_t bufferLength, int seed)
{
	char * buffer = new char[bufferLength];
	int i = 0;
	for (int n = seed; n < seed + bufferLength; n++)
	{
		buffer[i++] = n;
	}
	return buffer;
}

int main(int argc, char * argv[])
{

	cout << "SerialSimu. " << __TIMESTAMP__ << "\n";

	//char sendBuffer[100];// = { 0x41,0x42,0x43,0x44,0x45,0x46,0x47,0x48,0x49,0x50, 0x41,0x42,0x43,0x44,0x45,0x46,0x47,0x48,0x49,0x50 };
	CUeiSession mySession;
	vector<CUeiSerialWriter*> writers;
	vector<CUeiSerialReader*> readers;
	string deviceUri = "pdna://192.168.100.3/Dev3/";

	//for (int chIndex = 0; chIndex < 8; chIndex++)
	int chIndex = 2;
	{
		stringstream sstream;

		sstream << deviceUri << "com" << chIndex;

		try {

			CUeiDevice* device = CUeiDeviceEnumerator::GetDeviceFromResource(deviceUri);
			if (nullptr != device)
			{
				device->Reset();

				CUeiSerialPort* port = mySession.CreateSerialPort(sstream.str(),
					UeiSerialModeRS485FullDuplex,
					UeiSerialBitsPerSecond115200,
					UeiSerialDataBits8,
					UeiSerialParityNone,
					UeiSerialStopBits1,
					"");


				UeiDaq::CUeiSerialPort* sport = static_cast<UeiDaq::CUeiSerialPort*>(mySession.GetChannel(chIndex));
			}
			else
			{
				cout << "Can't reach " << sstream.str() << ". Program aborted \n";
				return 0;
			}

		}
		catch (exception &ex)
		{
			cout << ex.what() << "\n";
		}
		catch (...)
		{
			cout << "exception" << "\n";
		}
	}
	cout << deviceUri  << ". 8 serial channels defined. 115200, RS485FullDuplex" << "\n";
	//"at 57600bps, RS485FullDuplex" << "\n";
	mySession.ConfigureTimingForAsynchronousIO(WATERMARK, 0, TIMEOUT_uS, 0);
	mySession.GetTiming()->SetTimeout(10);

	//// Create an asynchronous listener and a writer for each port configured
	//// in the resource string
	//writer = new CUeiSerialWriter [mySession.GetNumberOfChannels()];
	//readers = new CUeiSerialReader * [mySession.GetNumberOfChannels()];
	//int com1 = 0;

	mySession.Start();

	for (int com = 0; com < mySession.GetNumberOfChannels(); com++)
	{
		int32_t chIndex = mySession.GetChannel(com)->GetIndex();
		writers.push_back(new CUeiSerialWriter(mySession.GetDataStream(), chIndex));
		readers.push_back(new CUeiSerialReader(mySession.GetDataStream(), chIndex));
	}
	
	if (argc > 1)
	{
		string argv1(argv[1]);
		if (argv1 == "-rx")
		{
			Int32 numBytesRead;
			char recvBuffer[300];

			//readers[0]->ReadTimestamped(200, recvBuffer, &numBytesRead);

			int portIndex = 2;
			//for (auto& reader : readers)
			auto& reader = readers[0];

			while (true)
			{
				{

					try 
					{
						int a = mySession.GetDataStream()->GetAvailableInputMessages(portIndex);
						if (a > 0)
						{
							reader->ReadTimestamped(300, recvBuffer, &numBytesRead);
							cout << numBytesRead << " bytes read." << "Ch " << portIndex << "\n";
						}
						//Sleep(1);
					}
					catch (CUeiException& e)
					{
						if (e.GetError() == UEIDAQ_TIMEOUT_ERROR)
						{
							printf("  read timeout\n");
							//break;
						}
						else
						{
							cout << e.GetErrorMessage() << "\n";
						}
					}
				}
		}
		}
	}


	// write loop
	// ==========
	int32_t  bufferLength = 16;
	for (int i = 0; i < 64; i++)
	{
		char * buffer = BuildBuffer(bufferLength, i + 32);
		cout << "Writing " << std::dec << bufferLength << " bytes on all channels. first byte:" << std::hex << static_cast<int32_t>(buffer[0]) << "\n";
		for (CUeiSerialWriter* writer : writers)
		{
			Int32 numberOfSent = 0;
			writer->Write(bufferLength, buffer, &numberOfSent);
			if (bufferLength != numberOfSent)
			{
				cout << "Write fail\n";
			}
		}
		delete buffer;
		Sleep(500);
	}


	for (auto& w : writers)
	{
		delete w;
	}
	for (auto& r : readers)
	{
		delete r;
	}

	//for (int w=0; w<writers.size(); w++)
	//{
	//	delete writers[w];
	//}
	mySession.Stop();

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
