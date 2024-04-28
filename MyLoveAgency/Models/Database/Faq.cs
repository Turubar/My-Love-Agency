using System;
using System.Collections.Generic;

namespace MyLoveAgency.Models.Database;

public partial class Faq
{
    public int Id { get; set; }

    public string QuestionEn { get; set; } = null!;

    public string QuestionUa { get; set; } = null!;

    public string AnswerEn { get; set; } = null!;

    public string AnswerUa { get; set; } = null!;
}
