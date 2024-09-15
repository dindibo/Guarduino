#include "MotionSensor.h"
#include "SecuritySystem.h"

#define SENSOR_PIN A0  // Pin where the motion sensor is connected

// Create an instance of the MotionSensor for detecting movement
MotionSensor motionSensor(SENSOR_PIN);

// We can easily add more sensors here in the future. Right now, just one.
Sensor* sensorArray[] = { &motionSensor };

// Create the SecuritySystem instance, pass the array of sensors (currently just one)
SecuritySystem securitySystem(sensorArray, 1);

// Function to read commands from serial input
String getCommand() {
  String command = "";
  
  // While there’s data in the serial buffer, read it
  while (Serial.available() > 0) {
    char c = Serial.read();  // Read one character at a time
    command += c;  // Build the full command string
    delay(10);  // Short delay to give time for the rest of the command to come in
  }
  return command;  // Return the assembled command
}


void setup() {
  Serial.begin(115200);
  // Set up done, nothing fancy needed here for now
}

void loop() {
  // Get any input command from the serial connection
  String command = getCommand();

  // Check if we received the special command to activate the system
  if (command == "\x00\x00\x00\x01") {
    // System is "armed" now, check if any sensors are triggered
    securitySystem.checkSensors();
  } 
  // Check if we got the command to send sensor data
  else if (command == "\x00\x00\x00\x02") {
    // This command sends the sensor's current data (useful for debugging)
    Serial.println(sensorArray[0]->getSensorData());
  }

  // A small delay so we don't overwhelm the system with constant checks
  delay(100);
}

