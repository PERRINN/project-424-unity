Hi Edy, 

I repeated the test we were talking about "424_5000_X_1000_Z" for the real tensor with different elements inside SIMDRIVE.
Basically I have different options for building structures, initially I chose the Flex-Body. 
It is the easiest option, because it is only one element, were I can implement an infinite number of 
mass-nodes and connect them via a matrix as I wish.
The connections could have been done flexible, or as was in our case, stiff.
It is also the fastest calculating option, but since we are talking here about 1-2 seconds its not really important.

Anyway, I have now build the exact same structure and test from separate 3D masses and a 3D connector, 
with the exact same specification as before, so you can compare the results.
But more importantly, since I now have the 3D masses instead of the Flex-Body, 
we now have the kinetic energy of the masspoint as a result,
and the guys were so kind and able to open also the angular velocity over all axis as well. 

I have written out four files. 
Please forgive the german language, the software is also available in english, but well I grew up in germany...
"Winkelbeschleunigung " is angular acceleration.
"Winkelgeschwindigkeit" is the angular velocity.
The rest is rather self explainig from the units.



