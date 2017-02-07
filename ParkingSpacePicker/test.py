from tkinter import *
from PIL import Image
from PIL import ImageTk
from tkinter import filedialog
import cv2
import numpy as np

def select_image():
    # grab a reference to the image panels
    global panelA, panelB, panelC

    # open a file choose dialog and allow the user to select an input
    # image
    path = filedialog.askopenfilename()

    # ensure a file path was selected
    if len(path) > 0:
        # Create the image for panelB
        # load the image from disk, convert it to grayscale, and detect
        # edges in it
        image = cv2.imread(path)
        gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
        edged = cv2.Canny(gray, 50, 100)

        # Create the image for panelA
        # OpenCV represents images in BGR order; however PIL represents
        # images in RGB order, so we need to swap the channels
        image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

        #Create the image for panelC
        boundries = [
            ([103, 86, 65], [145, 133, 128]),
        ]
        # create NumPy arrays from the boundries
        lower = np.array(boundries[0][0], dtype = "uint8")
        upper = np.array(boundries[0][1], dtype = "uint8")
        # find the colors within the specified boundries and apply
        # the mask
        mask = cv2.inRange(image, lower, upper)
        output = cv2.bitwise_and(image, image, mask = mask)


        # convert the images to PIL format...
        image = Image.fromarray(image)
        edged = Image.fromarray(edged)
        output = Image.fromarray(output)

        # ...and then to ImageTk format
        image = ImageTk.PhotoImage(image)
        edged = ImageTk.PhotoImage(edged)
        output = ImageTk.PhotoImage(output)


        # if the panels are None, initialize them
        if panelA is None or panelB is None or panelC is None:
            # root.minsize(width=666, height=666)
            # root.maxsize(width=666, height=666)

            # the first panel will store our original image
            # panelA = Label(canvas, image=image)
            # panelA.image = image
            # panelA.pack(side="top", padx=10, pady=10)
            canvas.create_image(0,0,anchor=NW,image=image)

            # while the second panel will store the edge map
            # panelB = Label(canvas, image=edged)
            # panelB.image = edged
            # panelB.pack(side="bottom", padx=10, pady=10)
            canvas.create_image(666,666,anchor=NW,image=edged)

            # panelC = Label(canvas, image=output)
            # panelC.image = output
            # panelC.pack(side="bottom", padx=10, pady=10)

            canvas.config(width=666, height=666)
        # otherwise, update the image panels
        else:
            # update the panels
            panelA.configure(image=image)
            panelB.configure(image=edged)
            panelC.configure(image=output)
            panelA.image = image
            panelB.image = edged
            panelC.image = output

# initialize the window toolkit along with the two image panels
root = Tk()
panelA = None
panelB = None
panelC = None

# root.resizable(width=False, height=False)

frame = Frame(root, width=666, height=666)
frame.grid(row=0,column=0)
# frame.pack(expand=1, fill=BOTH)

# inner_frame = Frame(frame)
# inner_frame.pack(side=LEFT)
# scrollbar = Scrollbar(root)
# scrollbar.pack( side = RIGHT, fill=Y )

canvas = Canvas(frame, width=666, height=666, bg='white', scrollregion=(0,0,666,2000))
canvas.pack(expand=1, fill=BOTH)

vbar = Scrollbar(frame,orient=VERTICAL)
vbar.pack(side=RIGHT, fill=Y)
vbar.config(command=canvas.yview)
canvas.config(yscrollcommand=vbar.set)

canvas.config(width=666, height=666)
# frame2 = Frame(root, width=600, height=600)
# frame2.pack( side = TOP )

frame1 = Frame(root, width=100, height=100, background="#b22222")
# frame1.pack(fill=None, expand=False)
frame1.grid(row=1,column=0)

# create a button, then when pressed, will trigger a file chooser
# dialog and allow the user to select an input image; then add the
# button to the GUI
btn = Button(frame1, text="Select an image", command=select_image)
btn.pack(side="bottom", fill="both", padx="10", pady="10")

# kick off the GUI
root.mainloop()
