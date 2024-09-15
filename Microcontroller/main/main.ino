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

void iterSiren(){
  if((currF >= SIREN_HIGH && risePitch) || (currF <= SIREN_LOW && !risePitch)){
    risePitch = !risePitch;
  }

  currF = currF + (risePitch ? 5 : -5);
  
  Serial.println(currF);
  tone(SPK, currF);
}

void resetSiren(){
  currF = SIREN_LOW;
  risePitch = true;
}

// Function to calculate the moving average
float calculateMovingAverage(float newValue) {
  // If the array is full, subtract the oldest value from the sum
  if (count == windowSize) {
    sum -= values[currentIndex];
  } else {
    count++;
  }

  // Add the new value to the sum
  sum += newValue;

  // Store the new value in the array2
  values[currentIndex] = newValue;

  // Update the current index, wrapping around if necessary
  currentIndex = (currentIndex + 1) % windowSize;

  // Return the moving average
  return sum / count;
}

// Threshold for detecting significant changes in distance (in cm)
const float movementThreshold = 15.0;

// Function to detect significant movement
bool detectMovement(float currentDistance, float previousDistance) {
  // Calculate the absolute difference between the current and previous distances
  float deltaDistance = abs(currentDistance - previousDistance);

  // If the change exceeds the threshold, return true to indicate movement
  if (deltaDistance > movementThreshold) {
    return true;
  } else {
    return false;
  }
}

// TODO: Use
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
  int rawVal = analogRead(A0);
  float procVal  = calculateMovingAverage(rawVal);


  float currentDistance = rawVal;  // Get the new distance reading

  if (cooldownPassed) {
    // Detect if there has been significant movement
    if (detectMovement(currentDistance, previousDistance)) {
      trig = TRIG_MAX;
    }

    if(trig){ 

      if(!lastTrig){
        // Rise trigger
        
      }

      iterSiren();
      
      --trig;  
    }
    else{     
      
      if(lastTrig || trig <= 0){
        // Fall trigger
        noTone(SPK);
        resetSiren();
        
      }
    }

    lastTrig = (trig > 0);

  }
  else {
    if (iterCount++ >= COOLDOWN_ITERATIONS) {
      cooldownPassed = true;
      Serial.println("Armed!");
    }
  }


  // Update previous distance for the next iteration
  previousDistance = currentDistance;

  delay(1);
}
