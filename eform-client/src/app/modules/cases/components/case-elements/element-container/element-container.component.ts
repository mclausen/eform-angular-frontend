import {Component, Input} from '@angular/core';
import {CaseDataItem} from 'app/models/cases/response/data-item.model';

@Component({
  selector: 'element-container',
  templateUrl: './element-container.component.html'
})
export class ElementContainerComponent {
  isCollapsed = true;
  dataItemList: Array<CaseDataItem> = [];
  @Input() dataItemLabel: string;
  @Input()
  get fieldValue() {
    return this.dataItemList;
  }
  set fieldValue(val) {
    this.dataItemList = val;
  }

  constructor() {
  }
}