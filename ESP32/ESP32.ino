#include <WiFi.h>
#include <WiFiUDP.h>
#include <BluetoothSerial.h>
#include <SPIFFS.h>
#include <Arduino.h>
#include "wifi_pass.h"

char deviceName[20];

BluetoothSerial SerialBT;

WiFiUDP udp;
const int udpPort = 60000;

void createWifiFile(char* ssid, char* pass) {
  SPIFFS.format();
  Serial.println("Creating wifi.txt file.");
  File wifiFile = SPIFFS.open("/wifi.txt", "w");
  if (wifiFile) {
    size_t ssidSize = strlen(ssid);
    size_t passSize = strlen(pass);
    for (size_t i = 0; i < ssidSize; i++) {
      wifiFile.write((uint8_t)ssid[i]);
    }
    wifiFile.write((uint8_t)'\n');
    for (size_t i = 0; i < passSize; i++) {
      wifiFile.write((uint8_t)pass[i]);
    }
    wifiFile.write((uint8_t)'\n');
    wifiFile.close();
  }
}

void connectToWiFi() {
  Serial.println("Reading Wi-Fi credentials from file.");

  File wifiFile = SPIFFS.open("/wifi.txt", "r");
  String savedSSID = wifiFile.readStringUntil('\n');
  String savedPassword = wifiFile.readStringUntil('\n');
  wifiFile.close();

  Serial.printf("Connecting to %s.\n",savedSSID.c_str());

  WiFi.begin(savedSSID.c_str(), savedPassword.c_str());

  while (WiFi.status() != WL_CONNECTED) {
    delay(100);
  }
    Serial.printf("Connected to %s.\n", savedSSID.c_str());
}

void setup() {
  Serial.begin(115200);

  uint32_t chipId = ESP.getEfuseMac();
  snprintf(deviceName, sizeof(deviceName), "LilWorker-%08X", chipId);

  Serial.printf("%s says hello!\n", deviceName);

  if (!SPIFFS.begin(true)) {
    Serial.println("SPIFFS file system initialization error! Freezing...");
    while (1)
      ;
  }

  if (!SPIFFS.exists("/wifi.txt")) {
    createWifiFile(WIFI_SSID, WIFI_PASS);

    SerialBT.begin(deviceName);

    Serial.printf("Bluetooth is on. Device name - %s\n",deviceName);

    SerialBT.enableSSP();

    //TODO: Repair this bluetooth shit. 
    //For the sake of testing, I will assume that everything went as expected.
    /*
    while (!SerialBT.connected()) {
      delay(100);
    }
    */

    Serial.println("Connected with somebody.");

    while (!SPIFFS.exists("/wifi.txt")) {
      if (SerialBT.available()) {
        //TODO: Data reading and saving to wifi.txt file. 
        char data = SerialBT.read();
        Serial.write(data);
      }
    }

    Serial.println("Turning off bluetooth.");

    SerialBT.end();
  }

  connectToWiFi();

  Serial.println("Starting UDP daemon.");

  udp.begin(udpPort);
}

//----------------------------------------------------------------

void blinkLed(int pin, int blinks, int time)
{
  pinMode(pin, OUTPUT);
  int delayTime = time/(2*blinks);
  for(int i = 0; i < blinks; i++)
  {
    digitalWrite(pin, HIGH);
    delay(delayTime);
    digitalWrite(pin,LOW);
    delay(delayTime);
  }
}

void replyToBroadcast() {
  int packetSize = udp.parsePacket();

  if (packetSize) {
    IPAddress senderIP = udp.remoteIP();
    uint16_t senderPort = udp.remotePort();

    char packetData[255];
    int bytesRead = udp.read(packetData, 255);

    if (bytesRead > 0) {
      packetData[bytesRead] = '\0';
      if (strcmp(packetData, "LilWorker?") == 0) {
        const uint8_t* response = (const uint8_t*)deviceName;
        udp.beginPacket(senderIP, senderPort);
        udp.write(response, strlen((const char*)response));
        udp.endPacket();
        Serial.printf("Introduced myself to %s:%d\n",senderIP.toString(), senderPort);
        blinkLed(2,2,1000);
        blinkLed(18,2,1000);
      }
    }
  }
}

void loop() {
  replyToBroadcast();
}

