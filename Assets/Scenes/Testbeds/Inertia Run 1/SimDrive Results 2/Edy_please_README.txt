Hi Edy, 

I repeated the test for the real tensor we were talking about yesterday ("424_5000_X_1000_Z") with different elements inside SIMDRIVE.
I have different options for building structures; initially, I chose the Flex-Body. 
It is the most comfortable option because it is only one element, where I can implement an infinite number of 
separate mass-nodes and connect them via a matrix as I wish.
The connections could have been done flexible, or as was in our case, stiff.
It is also the fastest calculating option, but since we are talking here about 1-2 seconds, it's not really important.

Anyway, I have now built the exact same structure and test from separate 3D masses and a 3D connector, 
with the exact same specification as before, so you can compare the results.
But more importantly, since I now have the 3D masses instead of the Flex-Body, 
we now have the kinetic energy of the mass point as a result,
and also the angular velocity over all axis as well. 
So you can now directly compare against unity.
The angular velocity usually is not a result that is written out, 
I stumbled over it by accident yesterday when I was looking for a way to get the kinetic energy,
and is only available for 3D masses, not the Flex-Body.

I have written out four files. 
Please forgive the german language, the software is also available in English, but well I grew up in Germany...
"Winkelbeschleunigung " is angular acceleration.
"Winkelgeschwindigkeit" is the angular velocity.
The rest is rather self-explaining from the units.

If this is going to work this way, I can repeat all simulations and write out the files again.
But maybe first start with one.

Best

Lukasz


