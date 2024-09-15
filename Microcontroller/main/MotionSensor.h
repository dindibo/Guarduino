#ifndef MOTION_SENSOR_H
#define MOTION_SENSOR_H

#include "Sensor.h"

// This is the motion sensor class. It checks for movement and tracks the sensor state.
class MotionSensor : public Sensor {
  private:
    int pin;  // Pin where the sensor is connected
    bool lastState;  // Keeps track of the last state (open/closed)
    
  public:
    // Constructor to initialize the sensor pin
    MotionSensor(int sensorPin) : pin(sensorPin), lastState(false) {
      pinMode(pin, INPUT);  // Set the pin as input
    }

    // Method to check if the sensor is triggered (could be motion, door, etc.)
    bool isTriggered() override {
      int sensorValue = analogRead(pin);  // Read the sensor value from the pin
      bool currentState = sensorValue > 500;  // If the value is above 500, we assume it’s triggered

      // If the sensor state changed, we return true (triggered)
      if (currentState != lastState) {
        lastState = currentState;  // Update the last known state
        return currentState;  // Return the new state
      }
      return false;  // No change, no trigger
    }

    // Get the current sensor data in serialized format
    String getSensorData() override {
      String state = lastState ? "open" : "closed";  // If lastState is true, it's open
      return "{'sensor_type': 'motion', 'sensor_name': 'motion_sensor', 'state':'" + state + "'}";
    }
};

#endif
