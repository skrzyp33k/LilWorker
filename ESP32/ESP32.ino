#include <WiFi.h>
#include <WebServer.h>
#include <SPIFFS.h>
#include <WiFiUDP.h>
#include <Arduino.h>

char deviceName[20];

const int RESET_PIN = 13;

WebServer server(80);
WiFiUDP udp;
const int udpPort = 60000;

// Sprawdzanie czy istnieje plik wifi.txt w pamięci
bool checkWifiFile() {
  return SPIFFS.exists("/wifi.txt");
}

// Łączenie z siecią WiFi na podstawie danych z pliku wifi.txt
void connectToWiFi() {
  File file = SPIFFS.open("/wifi.txt", "r");
  if (file) {
    String ssid = file.readStringUntil('\n');
    String password = file.readStringUntil('\n');
    ssid.trim();
    password.trim();
    file.close();

    WiFi.begin(ssid.c_str(), password.c_str());
    while (WiFi.status() != WL_CONNECTED) {
      delay(1000);
    }

    Serial.println("Połączono z siecią WiFi: " + ssid);
    Serial.print("Adres IP: ");
    Serial.println(WiFi.localIP());
  }
}

// Obsługa żądania HTTP POST
void handlePostRequest() {
  if (server.hasArg("ssid") && server.hasArg("password")) {
    String ssid = server.arg("ssid");
    String password = server.arg("password");

    File file = SPIFFS.open("/wifi.txt", "w");
    if (file) {
      file.println(ssid);
      file.println(password);
      file.close();
      server.send(200, "text/plain", "Zapisano dane WiFi");

      delay(2000);
      ESP.restart();
    }
  }
}


// Obsługa żądania HTTP GET
void handleGetRequest() {
  String html = "<!DOCTYPE html>"
                "<html>"
                "<head>"
                "<meta charset=\"UTF-8\"/>"
                "<title>"
                + String(deviceName)
                + "</title>"
                  "</head>"
                  "<body>"
                  "<h2>Ustawienia WiFi</h2>"
                  "<form method=\"post\" action=\"/save\">"
                  "<label for=\"ssid\">SSID:</label><br>"
                  "<input type=\"text\" id=\"ssid\" name=\"ssid\"><br><br>"
                  "<label for=\"password\">Hasło:</label><br>"
                  "<input type=\"password\" id=\"password\" name=\"password\"><br><br>"
                  "<input type=\"submit\" value=\"Zapisz\">"
                  "</form>"
                  "</body>"
                  "</html>";

  server.send(200, "text/html", html);
}

// Obsługa przycisku reset
void handleResetButton() {
  if (digitalRead(RESET_PIN) == LOW) {
    Serial.println("Formatowanie...");
    SPIFFS.format();
    Serial.println("Restartowanie...");
    ESP.restart();
  }
}

//Obsługa odpowiedzi na broadcast
void replyToBroadcast()
{
// Nasłuchiwanie pakietu UDP
  int packetSize = udp.parsePacket();
  if (packetSize) {
    char packetData[255];
    int len = udp.read(packetData, sizeof(packetData));
    if (len > 0) {
      packetData[len] = '\0';
      Serial.println("Otrzymano pakiet UDP: " + String(packetData));
      if (strcmp(packetData, "LilWorker?") == 0) {
        const uint8_t* response = (const uint8_t*)deviceName;
        udp.beginPacket(udp.remoteIP(), udp.remotePort());
        udp.write(response, strlen((const char*)response));
        udp.endPacket();
        Serial.print("Przedstawiono się ");
        Serial.print(udp.remoteIP());
        Serial.print(":");
        Serial.println(udp.remotePort());
      }
    }
  }
}

// Inicjalizacja serwera HTTP
void setup() {
  Serial.begin(115200);

  uint32_t chipId = ESP.getEfuseMac();
  snprintf(deviceName, sizeof(deviceName), "LilWorker-%08X", chipId);

  Serial.print(deviceName);
  Serial.println(" zgłasza gotowość do pracy!");

  if (!SPIFFS.begin(true)) {
    Serial.println("Błąd inicjalizacji SPIFFS");
    while (true) {
      delay(1);
    }
  }

  if (checkWifiFile()) {
    connectToWiFi();
  } else {
    WiFi.softAP(deviceName);
    Serial.print("Hotspot utworzony, Adres IP Hotspotu: ");
    Serial.println(WiFi.softAPIP());
  }

  server.on("/", HTTP_GET, handleGetRequest);
  server.on("/save", HTTP_POST, handlePostRequest);

  server.begin();

  udp.begin(udpPort);
  Serial.println("Nasłuchiwanie pakietów UDP na porcie " + String(udpPort));

  pinMode(RESET_PIN, INPUT_PULLUP);
}

//Główna pętla
void loop() {
  server.handleClient();

  replyToBroadcast();

  handleResetButton();
}