import requests
import json
import cloudinary
import os, shutil

# import sched, time
# s = sched.scheduler(time.time, time.sleep)
# def do_something(sc): 
    # print "Doing stuff..."
    # do your stuff
    # s.enter(60, 1, do_something, (sc,))

# s.enter(60, 1, do_something, (s,))
# s.run()

output_base = './output/'
jsonInput = { 'data': [] };

def getParkingInfo():
	# This will return a json object of all the parking spaces set up on the website
	parkingSpaces = requests.get('http://www.autospots.org/parking_spaces.json').json()

	jsonInput['count'] = len(parkingSpaces)

	# Do the following for every set up parking lot
	for space in parkingSpaces:
		pid = str(space['id'])
		# print pid
		coords = json.loads(space['parking_coords'])
		imgUrl = str(space['image_url'])

		# First create the folders in the output folder that the images will go into
		picDir = output_base + pid + '/'
		spotsDir = picDir	+ 'spots/'
		if not os.path.exists(output_base):
			os.mkdir(output_base)
		if not os.path.exists(picDir):
			os.mkdir(picDir)
		if not os.path.exists(spotsDir):
			os.mkdir(spotsDir)

		# Download the referenced image into the new created folder
		imgLoc = picDir+'orig.jpg'
		with open(imgLoc, 'wb') as pic:
			# print 'imgUrl:', imgUrl
			r = requests.get(imgUrl, stream=True)

			if not r.ok:
				print r

			for block in r.iter_content(512):
				if not block:
					break
				pic.write(block)
			# pic.write(r.content)

		# Create the json file from the selecting parking spots we downloaded
		jsonLoc = picDir+'parkingSpots.json'
		with open(jsonLoc, 'wb') as jsonFile:
			json.dump(coords, jsonFile)

		# Then we need to add the info the the inner object
		spaceOutput = { 
			'output':	picDir,
			'json': 	jsonLoc,
			'name': 	imgLoc
		}
		jsonInput['data'].append(spaceOutput)

	# save the object we created as a json file for the cv part to read
	with open('images.json', 'w') as fp:
		json.dump(jsonInput, fp)

def runCV():
	stuff

def cleanup():
	shutil.rmtree('./output/')


getParkingInfo()
print jsonInput