#include <WiFi.h>
#include <WiFiUDP.h>
#include <Arduino.h>
#include "wifi_pass.h"

const char* ssid = WIFI_SSID;
const char* password = WIFI_PASS;
const int udpPort = 60000;

char chipIdStr[9];

WiFiUDP udp;

void setup() {
  Serial.begin(115200);

  uint32_t chipId = ESP.getEfuseMac();
  snprintf(chipIdStr, sizeof(chipIdStr), "%08X", chipId);

  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.println("Connecting to WiFi...");
  }
  Serial.println("Connected to WiFi");

  udp.begin(udpPort);
}

void loop() {
  replyToBroadcast();
}

void replyToBroadcast() {
  int packetSize = udp.parsePacket();

  if (packetSize) {
    IPAddress senderIP = udp.remoteIP();
    uint16_t senderPort = udp.remotePort();

    Serial.print("Received packet from ");
    Serial.print(senderIP);
    Serial.print(":");
    Serial.println(senderPort);

    char packetData[255];
    int bytesRead = udp.read(packetData, 255);

    if (bytesRead > 0) {
      packetData[bytesRead] = '\0';
      if (strcmp(packetData, "LilWorker?") == 0) {
        const uint8_t* response = (const uint8_t*)chipIdStr;
        udp.beginPacket(senderIP, senderPort);
        udp.write(response, strlen((const char*)response));
        udp.endPacket();
        Serial.println("Sending response");
      }
    }
  }
}