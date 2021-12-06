# Parallel-Image-Processing


# Running compression:
  $ mono ImageProcessing <numThreads> <inputImageName> <outputImageNewName> compress <compressionValue from(0-100 inclusive)>
  
  ex:
    $ mono ImageProcessing 16 16k.jpeg commandlineout.jpeg compress 100


# Running brightness 
  $ mono ImageProcessing <numThreads> <inputImageName> <outputImageNewName> bri <brightnessValue (any postive or negative number)>

  ex:
    $ mono ImageProcessing 16 16k.jpeg commandlineout.jpeg bri 50


# Running black and white
  $ mono ImageProcessing <numThreads> <inputImageName> <outputImageNewName> baw
  
  ex:
    $ mono ImageProcessing 16 16k.jpeg commandlineout.jpeg baw