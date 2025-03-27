import pandas as pd
import numpy as np
import tensorflow as tf
from tensorflow import keras
from keras import layers;
import tf2onnx
import onnx

print("Reading the training data")
csv = pd.read_csv("data/training_data.csv", header=0, skiprows=[1])

inputs = csv[['Throttle', 'Brake', 'AccelerationLat', 'AccelerationLong', 'AccelerationVert', 
             'nWheelFL', 'nWheelFR', 'nWheelRL', 'nWheelRR', 'SteeringAngle']]
output = csv['Speed']


print("\nCreating the model");
speed_model = tf.keras.Sequential([
    layers.Dense(12, activation='relu'),
    layers.Dense(1)
])


speed_model.compile(loss = tf.keras.losses.MeanSquaredError(),
                      optimizer = tf.keras.optimizers.Adam())

print("\nTraining the model");
speed_model.fit(inputs, output, batch_size = 256, epochs=1000, validation_split=0.2)

# Evaluate the model
print("\nEvaluating on test data")
results = speed_model.evaluate(inputs, output, batch_size=10)
print("test loss, test acc:", results)

print("\nExporting to a Unity Sentis compatible format")
input_signature = [tf.TensorSpec(speed_model.inputs[0].shape, speed_model.inputs[0].dtype)]
onnx_model, _ = tf2onnx.convert.from_keras(speed_model, input_signature, opset=9)
onnx.save_model(onnx_model, "model/speed_estimator.onnx")
