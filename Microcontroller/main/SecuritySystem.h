#ifndef SECURITY_SYSTEM_H
#define SECURITY_SYSTEM_H

#include <Arduino.h>
#include "Sensor.h"

// This class manages the overall security system. It tracks the sensors and reacts when triggered.
class SecuritySystem {
  private:
    Sensor** sensors;  // Pointer array to hold the sensors
    int sensorCount;   // The number of sensors we are managing

  public:
    // Constructor to initialize with a list of sensors and the number of sensors
    SecuritySystem(Sensor** sensorArray, int count) : sensors(sensorArray), sensorCount(count) {}

    // This method loops through all the sensors and checks if any of them are triggered
    void checkSensors() {
      for (int i = 0; i < sensorCount; i++) {
        if (sensors[i]->isTriggered()) {
          triggerAlarm(sensors[i]);  // Trigger the alarm if any sensor is triggered
        }
      }
    }

    // Method to handle the alarm when a sensor is triggered
    void triggerAlarm(Sensor* triggeredSensor) {
      // For now, we just print the sensor data to the serial monitor
      Serial.println("Alarm triggered by sensor!");
      Serial.println(triggeredSensor->getSensorData());

      // You could integrate siren code or any alert mechanism here
      tone(10, 1000, 500);  // Just a quick tone to simulate an alarm
    }
};

#endif
