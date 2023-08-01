import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
baseuri='https://localhost:7035/Api/';
  constructor(private http: HttpClient) { }

  login(model:any)
  {
    return this.http.post(this.baseuri+'account/login',model);
  }
}
