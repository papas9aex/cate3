using System.Diagnostics.Metrics;
using System.Reflection;
using System.Reflection.Emit;
using Bogus;
using Bogus.DataSets;
using Npgsql;


class Person
{
    public Int32 Id { get; set; }
    public Guid TransportId { get; set; }
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public Int32 Age { get; set; }
    public string Phones { get; set; }
}

internal class PersonsGenerator
{
    public List<Person> persons = new List<Person>();
    Random random = new Random();

    PhoneNumbers phones = new PhoneNumbers();



    public PersonsGenerator(int count)
    {
        Faker name = new Faker();
        for (int i = 0; i < count; ++i)
        {
            persons.Add(new Person
            {
                Id = i,
                TransportId = Guid.NewGuid(),

                // Случайная генерация имен
                FirstName = name.Name.FirstName(),
                LastName = name.Name.LastName(),

                // Числа тоже можно генерировать
                Age = name.Random.Number(10, 90),
                Phones = phones.PhoneNumber(),
            });
        }

    }
}
internal class ConsoleWriter<T> where T : class
{
    public ConsoleWriter(List<T> collection)
    {
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var o in collection)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                Console.WriteLine($"{properties[i].Name}:\t{properties[i].GetValue(o)}");

                // Не обращаем внимание, так, для себя сделал
                if (properties[i].PropertyType.IsArray)
                {
                    Console.WriteLine("Array!");
                }

            }
            Console.WriteLine();
        }

    }
}
class Program
{
    static void Main(string[] args)
    {
            string connString = "Server=127.0.0.1;Port=5432;Database=3kt;User Id=postgres;Password=123";
            PersonsGenerator generator = new PersonsGenerator(10);
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                string insertSql = $"INSERT INTO Worker (worker_id,IDWorker, FirstName, LastName, Age, phones) VALUES (@worker_id, @IDWorker, @FirstName, @LastName, @Age, @phones)";

                foreach (var person in generator.persons)
                {
                    try
                    {
                        if (person.Age > 14)
                        {
                            // Создайте объект NpgsqlCommand
                            using (NpgsqlCommand cmd = new NpgsqlCommand(insertSql, conn))
                            {
                                // Установите параметры для SQL-запроса
                                cmd.Parameters.AddWithValue("@worker_id", person.Id);
                                cmd.Parameters.AddWithValue("@IDWorker", Guid.NewGuid());
                                cmd.Parameters.AddWithValue("@FirstName", person.FirstName);
                                cmd.Parameters.AddWithValue("@LastName", person.LastName);
                                cmd.Parameters.AddWithValue("@Age", person.Age);
                                cmd.Parameters.AddWithValue("@phones", person.Phones);

                                // Выполните SQL-запрос
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Invalid Age: {person.Age} for {person.FirstName} {person.LastName}. Skipping insertion.");
                        }
                    }
                    catch(WrongAge ex)
                    {
                        Console.WriteLine("Ты слишком мал для этой работы");
                    }

                }
            }
        
            ConsoleWriter<Person> debuger = new ConsoleWriter<Person>(generator.persons);
        Console.WriteLine("Reflection is end. Push ENTER to continue...");

        Console.ReadKey();
    }
}
public class WrongAge : ApplicationException
{
    public WrongAge() { }
    public WrongAge(string message) : base(message) { }
    public WrongAge(string message, Exception ex) : base(message) { }
}
