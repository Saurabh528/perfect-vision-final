using UnityEngine;

public class DiopterUtil
{
	public static float ConvertDisparityMMToDiopter(float disparityMM, float distanceToScreenMM)
	{
		//formula for getting Vergence demand(diopters)
		/*
		 
		                   Δx × 1000
                      D = ----------
		                      f×IOD
		D: Vergence demand (diopters).
		IOD: Interocular distance (typically 64 mm).
		Δx: Disparity (in mm).
		f: Viewing distance (in mm).

		 * 
		return disparityMM * 1000 / distanceToScreenMM / 64;*/
		/*
		 (LP X LS) - (TD X TS) = C
		Where:

				LP: lens power for the stereoscope (most are +5.00 D)

				LS: distance between the optical centers of the stereoscope lenses in centimeters

				TD: target distance IN DIOPTERS

				TS: target separation in centimeters

				C: total convergence demand

				positive number: net convergence demand

				negative number: net divergence demand

				2. LENS POWER (LP): most all Brewster type stereoscopes have +5.00 lenses mounted base out. This means that optical infinity is at 20 cm.

				a. the LP term, then is always +5.00 D

				b. for the most part, this term is a constant (for a standard Brewster stereoscope)

				3. DISTANCE BETWEEN THE OPTICAL CENTERS (LS): most stereoscopes have an optical center separation of 85 mm. Therefore, the LS term is usually 8.5 (cm). This, however , is not true for all stereoscopes.

				a. therefore,different vergence demands in stereoscopes may be created if the distance between the two optical centers is different from the "standard" 8.5 cm.

				b. the changes are usually not drastic, as the principle equation points out (a 10 mm change in optical center separation results in a 5Æ change in fusional vergence demand at a 20 cm viewing distance (optical infinity)

				c. in fact, the Keystone Telebinocular has a lens center separation of 95 mm which translates to a 5Æ increase in BO demand at a 20 cm viewing distance

				4. TARGET DISTANCE (TD): assuming the Brewster stereoscope has +5.00 lenses, a distance target (at optical infinity) should be placed at 20 cm. At that distance, the dioptric value is 5.00 D.

				a. some stereogram series are designed for near point work as well. The same principle equation can be used here. Specifically, 2.5 diopters of accommodative demand is produced in a stereoscope at a working distance of 13.3 cm. In this case, the TD term changes to 7.5D

				b. many practitioners will make use of the movable carrier on Brewster stereoscopes to change the accommodative and vergence demand of a particular stereogram. These changes are predicted by the Principle Equation for the Stereoscope

				i. as the carrier is moved closer to the patient: TD values increase

				- accommodative demand increases

				- convergence demand decreases OR divergence demand increases (this is predicted by the formula....why this happens will be discussed in the next section)

				ii. as the carrier is moved away from the patient: TD values decrease

				- accommodative demand decreases

				- convergence demand increases OR divergence demand decreases (this is predicted by the formula....why this happens will be discussed in the next section)

 

				5. TARGET SEPARATION: vergence demand can be changed by altering the target separation between the two half fields of a stereogram. In the case of the stereoscope, all fusion is uncrossed (orthoptic). The amount of convergence or divergence demand for a particular stereocard is dictated by target separation and may be calculated using the Principle Equation for the Stereoscope.

				a. the Principle Equation for the Stereoscope may be used to calculate the ortho vergence demand for a particular stereogram.

				At optical infinity:

				(5.00 D X 8.5 cm) - (5.00 D X TS) = 0 demand

 

				TS = 5.00 D X 8.5 cm/5.00 D

				TS = 8.5 cm

				At nearpoint (13.3 cm):

				(5.00 D X 8.5 cm) - (7.50 D X TS) = 0 demand

 

				TS = 5.00 D X 8.5 cm/7.50 D

				TS = 5.67 cm
						 
		return 5 * 8.5f - 100 / (distanceToScreenMM / 10) * disparityMM / 10;*/
		return Mathf.Abs(disparityMM) * 100 / distanceToScreenMM;
	}
}
