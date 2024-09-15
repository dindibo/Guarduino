#define SPK (10)
#define COOLDOWN_ITERATIONS (2000)
#define TRIG_MAX (1000)

#define SIREN_LOW (440)
#define SIREN_HIGH (1000)

// Define the size of the window for the moving average
const int windowSize = 20;
float values[windowSize];   // Array to store the last N measured values
int currentIndex = 0;       // Current position in the array
float sum = 0;              // Sum of the current values in the window
int count = 0;              // Number of valid values (for the first N measurements)
float previousDistance = 0;

bool cooldownPassed = false;
int iterCount = 0;
int trig = 0;
bool lastTrig = false;

// Siren
int currF = SIREN_LOW;
bool risePitch = true;
bool systemEnabled = false;  // Variable to track system state

void iterSiren() {
  if ((currF >= SIREN_HIGH && risePitch) || (currF <= SIREN_LOW && !risePitch)) {
    risePitch = !risePitch;
  }
  currF = currF + (risePitch ? 5 : -5);
  Serial.println(currF);
  tone(SPK, currF);
}

void resetSiren() {
  currF = SIREN_LOW;
  risePitch = true;
}

// Function to calculate the moving average
float calculateMovingAverage(float newValue) {
  if (count == windowSize) {
    sum -= values[currentIndex];
  } else {
    count++;
  }
  sum += newValue;
  values[currentIndex] = newValue;
  currentIndex = (currentIndex + 1) % windowSize;
  return sum / count;
}

// Threshold for detecting significant changes in distance (in cm)
const float movementThreshold = 15.0;

// Function to detect significant movement
bool detectMovement(float currentDistance, float previousDistance) {
  float deltaDistance = abs(currentDistance - previousDistance);
  return (deltaDistance > movementThreshold);
}

// Function to read command from serial
String getCommand() {
  String command = "";
  while (Serial.available() > 0) {
    char c = Serial.read();
    command += c;
    delay(10);  // To allow full command reception
  }
  return command;
}

// Serialize sensor data
void sendSensorData() {
  String state = (trig > 0) ? "open" : "closed";
  String sensorData = "{'sensor_type': '0', 'sensor_name': 'salon', 'state':'" + state + "'}";
  Serial.println(sensorData);  // Send serialized data to serial
}

// Main logic state
enum MACHINE_STATE {
  STATE_SAFE,
  STATE_ARMING,
  STATE_ARMED,
  STATE_ALARM,
} typedef MACHINE_STATE;

MACHINE_STATE state = STATE_SAFE;

void setup() {
  pinMode(A0, INPUT);
  Serial.begin(115200);
}

void loop() {
  // Check for command from serial
  String command = getCommand();
  if (command == "\x00\x00\x00\x01") {
    systemEnabled = true;  // Enable the system when the magic string is received
  } else if (command == "\x00\x00\x00\x02") {
    sendSensorData();  // Send serialized data when the other magic string is received
  }

  if (systemEnabled) {
    int rawVal = analogRead(A0);
    float procVal = calculateMovingAverage(rawVal);
    float currentDistance = rawVal;  // Get the new distance reading

    if (cooldownPassed) {
      // Detect if there has been significant movement
      if (detectMovement(currentDistance, previousDistance)) {
        trig = TRIG_MAX;
      }

      if (trig) {
        if (!lastTrig) {
          // Rise trigger
        }
        iterSiren();
        --trig;
      } else {
        if (lastTrig || trig <= 0) {
          // Fall trigger
          noTone(SPK);
          resetSiren();
        }
      }
      lastTrig = (trig > 0);
    } else {
      if (iterCount++ >= COOLDOWN_ITERATIONS) {
        cooldownPassed = true;
        Serial.println("Armed!");
      }
    }
    previousDistance = currentDistance;
  }
  delay(1);
}
