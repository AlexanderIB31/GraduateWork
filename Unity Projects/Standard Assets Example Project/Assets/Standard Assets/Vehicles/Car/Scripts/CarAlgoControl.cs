using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System;
using System.Collections.Generic;
using System.Globalization;


namespace UnityStandardAssets.Vehicles.Car
{
	[RequireComponent(typeof(CarController))]
	public class CarAlgoControl : MonoBehaviour
	{
		//private bool m_Driving;
		private CarController m_CarController;
		[SerializeField]
		private Rigidbody entry_Rigidbody;
		[SerializeField]
		private Transform entry_Target;
		private Rigidbody m_Rigidbody;
		private Solution m_Solution;
	

		private void Awake()
		{
			m_CarController = GetComponent<CarController>();
			m_Rigidbody = GetComponent<Rigidbody>();
			List<double> parameters;
			// Считываем параметры для Solution из файла params.txt, сгенерированным проектом GA
			using (var fr = new System.IO.StreamReader("C:\\Users\\Alex31\\Documents\\Visual Studio 2015\\Projects\\GraduateWork\\params.txt"))
			{
				parameters = new List<double>(Array.ConvertAll(fr.ReadToEnd().TrimEnd('|').Replace(',', '.').Split('|'), double.Parse));
			}

			m_Solution = new Solution(parameters.ToArray(),
										30, 
										(entry_Rigidbody.position - m_Rigidbody.position).magnitude,
										0, 0);
		}

		private void FixedUpdate()
		{
				//Vector3 fwd = transform.forward;
				//if (m_Rigidbody.velocity.magnitude > m_CarController.MaxSpeed * 0.1f)
				//{
				//	fwd = m_Rigidbody.velocity;
				//}

				//float desiredSpeed = m_CarController.MaxSpeed;

				//var scalVec = Vector3.Scale(m_Rigidbody.velocity, entry_Rigidbody.velocity);
				//var cos = (scalVec / (m_Rigidbody.velocity.magnitude * entry_Rigidbody.velocity.magnitude)).magnitude;
				//var angle = Mathf.Acos(cos);

				// the car will brake as it approaches its target, regardless of the target's direction. Useful if you want the car to
				// head for a stationary target and come to rest when it arrives there.

				// check out the distance to target
				var delta = (entry_Target.position - m_Rigidbody.position).magnitude;

				var resAccel = (float)m_Solution.ToSolveNow(entry_Rigidbody.velocity.magnitude,
												m_CarController.CurrentSpeed,
												delta);

				// decide the actual amount of accel/brake input to achieve desired speed.
				float accel = Mathf.Clamp(resAccel, -1, 1);

				//// add acceleration 'wander', which also prevents AI from seeming too uniform and robotic in their driving
				//// i.e. increasing the accel wander amount can introduce jostling and bumps between AI cars in a race
				//accel *= (1 - m_AccelWanderAmount) +
				//		 (Mathf.PerlinNoise(Time.time * m_AccelWanderSpeed, m_RandomPerlin) * m_AccelWanderAmount);

				//// calculate the local-relative position of the target, to steer towards
				//Vector3 localTarget = transform.InverseTransformPoint(offsetTargetPos);

				//// get the amount of steering needed to aim the car towards the target
				//float steer = Mathf.Clamp(angle * m_SteerSensitivity, -1, 1) * Mathf.Sign(m_CarController.CurrentSpeed);

				//// feed input to the car controller.
				m_CarController.Move(0, accel, accel, 0f);

				//// if appropriate, stop driving when we're close enough to the target.
				//if (m_StopWhenTargetReached && localTarget.magnitude < m_ReachTargetThreshold)
				//{
				//	m_Driving = false;
				//}
		}

		public void SetTarget(Transform target)
		{
			entry_Target = target;
			//m_Driving = true;
		}

		public void SetRigidbody(Rigidbody rigidbody)
		{
			entry_Rigidbody = rigidbody;
		}
	}

	public class Solution
	{
		// Особые точки для dS
		private readonly double _x1;
		private readonly double _x2;
		private readonly double _x3;
		private readonly double _x4;
		private readonly double _x5;
		private readonly double _x6;
		private readonly double _x7;
		// Особые точки для dV
		private readonly double _z1;
		private readonly double _z2;
		private readonly double _z3;
		private readonly double _z4;
		private readonly double _z5;
		private readonly double _z6;
		private readonly double _z7;
		// Особые точки для V*
		private readonly double _y1;
		private readonly double _y2;
		private readonly double _y3;
		private readonly double _y4;
		private readonly double _y5;

		// Скорость в (м/с)
		private double _mySpeed = 0;
		// Цель движется неравномерно
		private double _entrySpeed = 16.7;
		// Требуемая дистанция в (м)
		private double _perfectDistance = 30;
		// Текущая дистанция в (м)
		private double _currentDistance = 300;
		// Частота вычислений: 1 (сек)
		private double _teta = 1;
		// Коэф. при вычислении ускорения
		private double _lambda = 0.98;
		private Dictionary<string, double> _rules = new Dictionary<string, double>();

		public Solution(double[] args, double perfectDist, double curDist, double mySpeed, double entrySpeed)
		{
			try
			{
				_x1 = args[0];
				_x2 = args[1];
				_x3 = args[2];
				_x4 = args[3];
				_x5 = args[4];
				_x6 = args[5];
				_x7 = args[6];

				_z1 = args[7];
				_z2 = args[8];
				_z3 = args[9];
				_z4 = args[10];
				_z5 = args[11];
				_z6 = args[12];
				_z7 = args[13];

				_y1 = args[14];
				_y2 = args[15];
				_y3 = args[16];
				_y4 = args[17];
				_y5 = args[18];

				_perfectDistance = perfectDist;
				_currentDistance = curDist;
				_mySpeed = mySpeed;
				_entrySpeed = entrySpeed;
				_lambda = args[19];
			}
			catch (ArgumentOutOfRangeException ex)
			{
				throw new ArgumentException("! проверьте кол-во передаваемых в args параметров, их должно быть 20 !");
			}
		}

		private double CloseDistance(double x)
		{
			if (_x1 <= x && x <= _x3)
			{
				return (_x3 - x) / (_x3 - _x1);
			}
			if (x <= _x1)
			{
				return 1;
			}
			return 0;
		}

		private double ZeroDistance(double x)
		{
			if (_x2 <= x && x <= _x4)
			{
				return (x - _x2) / (_x4 - _x2);
			}
			if (_x4 <= x && x <= _x6)
			{
				return (_x6 - x) / (_x6 - _x4);
			}
			return 0;
		}

		private double FarDistance(double x)
		{
			if (_x5 <= x && x <= _x7)
			{
				return (x - _x5) / (_x7 - _x5);
			}
			if (_x7 <= x)
			{
				return 1;
			}
			return 0;
		}

		private double LessSpeed(double z)
		{
			if (_z1 <= z && z <= _z3)
			{
				return (_z3 - z) / (_z3 - _z1);
			}
			if (z <= _z1)
			{
				return 1;
			}
			return 0;
		}

		private double ZeroSpeed(double z)
		{
			if (_z2 <= z && z <= _z4)
			{
				return (z - _z2) / (_z4 - _z2);
			}
			if (_z4 <= z && z <= _z6)
			{
				return (_z6 - z) / (_z6 - _z4);
			}
			return 0;
		}

		private double MoreSpeed(double z)
		{
			if (_z5 <= z && z <= _z7)
			{
				return (z - _z5) / (_z7 - _z5);
			}
			if (_z7 <= z)
			{
				return 1;
			}
			return 0;
		}

		public double ToSolveNow(double entrySpeed, double mySpeed, double curDistance)
		{
			var deltaDistance = _perfectDistance == 0 ? 0 : (curDistance - _perfectDistance) / _perfectDistance;
			var deltaSpeed = entrySpeed == 0 ? 0 : (mySpeed - entrySpeed) / entrySpeed ;
			// Дефазификация осуществляется по методу Мамдани
			#region Fuzzification
			// Три области фазиффикации: близко (прямоуг трапеция), средне (треугольник), далеко (прямоуг. трапеция)
			double A = CloseDistance(deltaDistance);
			double B = ZeroDistance(deltaDistance);
			double C = FarDistance(deltaDistance);
			double D = LessSpeed(deltaSpeed);
			double E = ZeroSpeed(deltaSpeed);
			double F = MoreSpeed(deltaSpeed);
			#endregion
			#region InferenceRule
			// Правило вывода: прямое соответствие расстояние и скорость - коэффициент лямбда
			// ++ - Сильно Увеличить
			// -- - Сильно Снизить
			// + - Немного Увеличить
			// - - Немного Уменьшить
			// 0 - Ничего не делать
			// dS\dV	-	0	+
			// -		0	-	--
			// 0		+	0	-
			// +		++	+	0
			_rules["CloseDist"] = A;
			_rules["ZeroDist"] = B;
			_rules["FarDist"] = C;
			_rules["LessSpeed"] = D;
			_rules["ZeroSpeed"] = E;
			_rules["MoreSpeed"] = F;
			#endregion
			#region Defuzzification
			// Дефазиффикация осуществляется методом Среднего Центра
			var resSpeed = (_y3 * Math.Min(_rules["CloseDist"], _rules["LessSpeed"])
						   + _y2 * Math.Min(_rules["CloseDist"], _rules["ZeroSpeed"])
						   + _y1 * Math.Min(_rules["CloseDist"], _rules["MoreSpeed"])
							+ _y4 * Math.Min(_rules["ZeroDist"], _rules["LessSpeed"])
							+ _y3 * Math.Min(_rules["ZeroDist"], _rules["ZeroSpeed"])
							+ _y2 * Math.Min(_rules["ZeroDist"], _rules["MoreSpeed"])
							+ _y5 * Math.Min(_rules["FarDist"], _rules["LessSpeed"])
							+ _y4 * Math.Min(_rules["FarDist"], _rules["ZeroSpeed"])
							+ _y3 * Math.Min(_rules["FarDist"], _rules["MoreSpeed"]))
					   / (Math.Min(_rules["CloseDist"], _rules["LessSpeed"])
						   + Math.Min(_rules["CloseDist"], _rules["ZeroSpeed"])
						   + Math.Min(_rules["CloseDist"], _rules["MoreSpeed"])
						   + Math.Min(_rules["ZeroDist"], _rules["LessSpeed"])
						   + Math.Min(_rules["ZeroDist"], _rules["ZeroSpeed"])
						   + Math.Min(_rules["ZeroDist"], _rules["MoreSpeed"])
						   + Math.Min(_rules["FarDist"], _rules["LessSpeed"])
						   + Math.Min(_rules["FarDist"], _rules["ZeroSpeed"])
						   + Math.Min(_rules["FarDist"], _rules["MoreSpeed"]));
			#endregion

			return _lambda * (resSpeed - mySpeed);
		}
	}
}
