# Autospots Computer Vision

To run: style a json file like demo.json. It should reference a json file of the parking lot layout as well as the image file to analyse. 

Then run python script like so...
```
pthon Autospots.py -i 'input.json'
```

## How it works

This code analyzes a parking lot and finds the empty parking spaces. The base code is built off of opencv and a project [Parking-Lot-Occupancy-Tracking](https://github.com/rugbyprof/Parking-Lot-Occupancy-Tracking). We have imporoved the methods they used with some techniques of our own as well as 2 different machine learning algorithms.

## Machine learning

We use segmantic image segmenation done with Conditional Random Fields as Recurrent Neural Networks. More information on this can be found on the project's [github](https://github.com/torrvision/crfasrnn) or their [demo website](http://crfasrnn.torr.vision)

We also use darknet, an open source neural network framework. They have implemented a object detection algorithm yolo that applies a neural network to the entire image and works from there finding objects faster than Fast-RCNN. Here is the links for their [github](https://github.com/pjreddie/darknet) and [website](https://pjreddie.com/darknet/)